// © 2026, Worth Systems.

using Common.Settings.Configuration;
using Common.Settings.Extensions;
using Common.Tests.Utilities._TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using System.Text.Json;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Clients.Factories.Interfaces;
using WebQueries.DataSending.Clients.Interfaces;
using WebQueries.DataSending.Models.Reponses;
using WebQueries.MOBB;
using WebQueries.MOBB.Interfaces;
using WebQueries.MOBB.Models;
using ZgwModels.Enums;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.OpenVtb;
using ZgwModels.Serialization.Interfaces;

namespace WebQueries.Tests.Unit.MOBB
{
    /// <summary>
    /// The Berichtenbox letter with <c>BRP_GEBRUIK_TESTADRES</c> switched on (#264).
    /// </summary>
    /// <remarks>
    ///   Kept apart from <see cref="MessageBoxScenarioImplementationTests"/> because the switch is a process-wide
    ///   environment variable.
    /// </remarks>
    [TestFixture]
    public sealed class MessageBoxScenarioBrpTestAddressTests
    {
        private static readonly Guid s_messageUuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Uri s_messageTypeUri = new("https://openvtb.test/berichten/api/v1/berichttypen/33333333-3333-3333-3333-333333333333");
        private const string TestBsn = "123456789";

        private OmcConfiguration _configuration = null!;
        private Mock<IQueryContext> _mockedQueryContext = null!;
        private Mock<INotifyClient> _mockedNotifyClient = null!;
        private IMessageBoxScenario _scenario = null!;

        [SetUp]
        public void SetupTests()
        {
            Environment.SetEnvironmentVariable(ConfigExtensions.BrpUseTestAddress, "true");

            this._configuration = ConfigurationHandler.GetOmcConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.ValidEnvironment_v2);

            var mockedQueryContext = this._mockedQueryContext = new Mock<IQueryContext>(MockBehavior.Strict);
            mockedQueryContext
                .Setup(mock => mock.GetVtbMessageAsync(It.IsAny<Uri>()))
                .ReturnsAsync(new VtbMessage
                {
                    MessageText = "Hello citizen",
                    RecipientUrn = $"urn:nld:bsn:{TestBsn}",
                    MessageTypeUri = s_messageTypeUri
                });
            mockedQueryContext
                .Setup(mock => mock.GetVtbMessageTypeAsync(s_messageTypeUri))
                .ReturnsAsync(new VtbMessageType
                {
                    Url = s_messageTypeUri,
                    MessageBoxType = "allowed-type",
                    IsInMyGovernmentMessageBox = false
                });
            mockedQueryContext
                .Setup(mock => mock.GetPartyDataAsync(null, TestBsn, null, false, true))
                .ReturnsAsync(new CommonPartyData
                {
                    Uri = new Uri("https://openklant.test/partijen/22222222-2222-2222-2222-222222222222"),
                    Name = "Jane",
                    Surname = "Doe",
                    EmailAddress = string.Empty  // Straight to the letter
                });

            var mockedDataQuery = new Mock<IDataQueryService<NotificationEvent>>(MockBehavior.Strict);
            mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(mockedQueryContext.Object);

            this._mockedNotifyClient = new Mock<INotifyClient>(MockBehavior.Strict);
            this._mockedNotifyClient
                .Setup(mock => mock.FindNotificationByReferenceAsync(It.IsAny<string>(), NotifyMethods.Letter))
                .ReturnsAsync(NotifyLookupResponse.NotFound());
            this._mockedNotifyClient
                .Setup(mock => mock.SendLetterAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), null))
                .ReturnsAsync(NotifySendResponse.Success());

            var mockedNotifyClientFactory = new Mock<IHttpClientFactory<INotifyClient, string>>(MockBehavior.Strict);
            mockedNotifyClientFactory
                .Setup(mock => mock.GetHttpClient(It.IsAny<string>()))
                .Returns(this._mockedNotifyClient.Object);

            var mockedSerializer = new Mock<ISerializationService>(MockBehavior.Strict);
            mockedSerializer
                .Setup(mock => mock.Serialize(It.IsAny<MessageBoxNotifyReference>()))
                .Returns("test-reference");

            // Strict and without setups: BRP must not be resolved, let alone called.
            var mockedServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);

            this._scenario = new MessageBoxScenarioImplementation(
                mockedDataQuery.Object,
                mockedNotifyClientFactory.Object,
                mockedSerializer.Object,
                this._configuration,
                mockedServiceProvider.Object,
                NullLogger<MessageBoxScenarioImplementation>.Instance);
        }

        [TearDown]
        public void CleanUpTests()
        {
            Environment.SetEnvironmentVariable(ConfigExtensions.BrpUseTestAddress, null);
            this._configuration.Dispose();
        }

        [Test]
        public async Task ProcessCloudEventAsync_TestAddressOn_SendsTheLetterToWorthSystems_WithoutBrp()
        {
            // Arrange
            JsonElement cloudEvent = JsonSerializer.SerializeToElement(new
            {
                type = "nl.overheid.berichten.bericht-gepubliceerd",
                subject = s_messageUuid.ToString()
            });

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(cloudEvent);

            // Assert
            Assert.That(result.IsSuccess, Is.True, result.JsonResponse);
            this._mockedNotifyClient.Verify(mock => mock.SendLetterAsync(
                It.IsAny<string>(),
                It.Is<Dictionary<string, object>>(personalization =>
                    (string)personalization["address_line_1"] == "Worth Systems t.a.v. OMC-test" &&
                    (string)personalization["address_line_2"] == "Laan van Vredenoord 11" &&
                    (string)personalization["address_line_3"] == "2289 DA Rijswijk"),
                It.IsAny<string>(),
                null), Times.Once);
        }

        [Test]
        public async Task HandleDeliveryFailureAsync_MobbFailed_EmailNotAcceptedByNotify_LetterDoesNotSayTheEmailFailed()
        {
            // NOTE: Notify NL did not accept the e-mail, so it was never sent: the letter goes out (#243), but not as
            //       a failed notificatie ("wasNotificatie"), which its template would tell the citizen.

            // Arrange
            this._mockedQueryContext
                .Setup(mock => mock.GetPartyDataAsync(null, TestBsn, null, false, true))
                .ReturnsAsync(new CommonPartyData
                {
                    Uri = new Uri("https://openklant.test/partijen/22222222-2222-2222-2222-222222222222"),
                    Name = "Jane",
                    Surname = "Doe",
                    EmailAddress = "citizen@example.com"
                });
            this._mockedNotifyClient
                .Setup(mock => mock.FindNotificationByReferenceAsync(It.IsAny<string>(), NotifyMethods.Email))
                .ReturnsAsync(NotifyLookupResponse.NotFound());
            this._mockedNotifyClient
                .Setup(mock => mock.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Failure("503 Service Unavailable"));

            var reference = new MessageBoxNotifyReference { MessageId = s_messageUuid, PartyId = Guid.NewGuid(), Mobb = true };

            // Act
            HttpRequestResponse result = await this._scenario.HandleDeliveryFailureAsync(reference, NotifyMethods.Mobb);

            // Assert
            Assert.That(result.IsSuccess, Is.True, result.JsonResponse);
            this._mockedNotifyClient.Verify(mock => mock.SendLetterAsync(
                It.IsAny<string>(),
                It.Is<Dictionary<string, object>>(personalization => (bool)personalization["wasNotificatie"] == false),
                It.IsAny<string>(),
                null), Times.Once);
        }

        [TestCase("1", true)]
        [TestCase(" TRUE ", true)]
        [TestCase("false", false)]
        [TestCase(null, false)]
        public void IsBrpTestAddressEnabled_ReadsTheSwitch(string? value, bool expected)
        {
            Environment.SetEnvironmentVariable(ConfigExtensions.BrpUseTestAddress, value);

            Assert.That(ConfigExtensions.IsBrpTestAddressEnabled(), Is.EqualTo(expected));
        }
    }
}
