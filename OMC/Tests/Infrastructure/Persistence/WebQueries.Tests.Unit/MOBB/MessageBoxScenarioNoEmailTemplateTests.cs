// © 2026, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using System.Text.Json;
using WebQueries.BRP;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Clients.Factories.Interfaces;
using WebQueries.DataSending.Clients.Interfaces;
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
    /// The Berichtenbox flow in a deployment that has no e-mail template for its digitale-post step (#263).
    /// </summary>
    /// <remarks>
    ///   Kept apart from <see cref="MessageBoxScenarioImplementationTests"/> because it needs a configuration with
    ///   one environment variable unset, and the configuration caches its values statically.
    /// </remarks>
    [TestFixture]
    public sealed class MessageBoxScenarioNoEmailTemplateTests
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
            // Clears whatever another fixture left in the static configuration cache.
            ConfigurationHandler.GetOmcConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.ValidEnvironment_v2).Dispose();

            this._configuration = ConfigurationHandler.GetValidEnvironmentConfigurationWith(
                new Dictionary<string, string?> { ["NOTIFY_TEMPLATEID_EMAIL_MESSAGEBOX"] = null });

            this._mockedQueryContext = new Mock<IQueryContext>(MockBehavior.Strict);
            this._mockedQueryContext
                .Setup(mock => mock.GetVtbMessageAsync(It.IsAny<Uri>()))
                .ReturnsAsync(new VtbMessage
                {
                    MessageText = "Hello citizen",
                    RecipientUrn = $"urn:nld:bsn:{TestBsn}",
                    MessageTypeUri = s_messageTypeUri
                });
            this._mockedQueryContext
                .Setup(mock => mock.GetVtbMessageTypeAsync(s_messageTypeUri))
                .ReturnsAsync(new VtbMessageType
                {
                    Url = s_messageTypeUri,
                    MessageBoxType = "allowed-type",
                    IsInMyGovernmentMessageBox = false
                });
            this._mockedQueryContext
                .Setup(mock => mock.GetPartyDataAsync(null, TestBsn, null, false, true))
                .ReturnsAsync(new CommonPartyData
                {
                    Uri = new Uri("https://openklant.test/partijen/22222222-2222-2222-2222-222222222222"),
                    Name = "Jane",
                    Surname = "Doe",
                    EmailAddress = "citizen@example.com"
                });

            var mockedDataQuery = new Mock<IDataQueryService<NotificationEvent>>(MockBehavior.Strict);
            mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            // Strict and without setups: no e-mail may be sent, nor anything else.
            this._mockedNotifyClient = new Mock<INotifyClient>(MockBehavior.Strict);

            var mockedNotifyClientFactory = new Mock<IHttpClientFactory<INotifyClient, string>>(MockBehavior.Strict);
            mockedNotifyClientFactory
                .Setup(mock => mock.GetHttpClient(It.IsAny<string>()))
                .Returns(this._mockedNotifyClient.Object);

            // BRP unavailable: reaching it proves the flow went on to the letter.
            var mockedServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            mockedServiceProvider.Setup(mock => mock.GetService(typeof(BrpClient))).Returns(null!);

            this._scenario = new MessageBoxScenarioImplementation(
                mockedDataQuery.Object,
                mockedNotifyClientFactory.Object,
                new Mock<ISerializationService>(MockBehavior.Strict).Object,
                this._configuration,
                mockedServiceProvider.Object,
                NullLogger<MessageBoxScenarioImplementation>.Instance);
        }

        [TearDown]
        public void CleanUpTests()
        {
            this._configuration.Dispose();
        }

        [Test]
        public async Task ProcessCloudEventAsync_NoEmailTemplate_SkipsTheEmailStep_GoesStraightToTheLetter()
        {
            // Arrange
            JsonElement cloudEvent = JsonSerializer.SerializeToElement(new
            {
                type = "nl.overheid.berichten.bericht-gepubliceerd",
                subject = s_messageUuid.ToString()
            });

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(cloudEvent);

            // Assert: the citizen has an e-mail address, yet nothing went to Notify NL and the letter was next
            Assert.That(result.JsonResponse, Does.Contain("BRP"));
            this._mockedNotifyClient.VerifyNoOtherCalls();
        }

        [Test]
        public async Task HandleDeliveryFailureAsync_MobbFailed_NoEmailTemplate_SkipsTheEmailStep_GoesStraightToTheLetter()
        {
            // Arrange
            var reference = new MessageBoxNotifyReference { MessageId = s_messageUuid, PartyId = Guid.NewGuid(), Mobb = true };

            // Act
            HttpRequestResponse result = await this._scenario.HandleDeliveryFailureAsync(reference, NotifyMethods.Mobb);

            // Assert
            Assert.That(result.JsonResponse, Does.Contain("BRP"));
            this._mockedNotifyClient.VerifyNoOtherCalls();
        }
    }
}
