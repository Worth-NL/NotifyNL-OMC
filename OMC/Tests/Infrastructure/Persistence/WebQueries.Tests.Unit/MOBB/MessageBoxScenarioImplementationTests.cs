// © 2026, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Notify.Models;
using NUnit.Framework;
using System.Text.Json;
using WebQueries.BRP;
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
using ZgwModels.Mapping.Models.POCOs.OpenZaak.Documents;
using ZgwModels.Serialization.Interfaces;

namespace WebQueries.Tests.Unit.MOBB
{
    [TestFixture]
    public sealed class MessageBoxScenarioImplementationTests
    {
        private Mock<IDataQueryService<NotificationEvent>> _mockedDataQuery = null!;
        private Mock<IQueryContext> _mockedQueryContext = null!;
        private Mock<IHttpClientFactory<INotifyClient, string>> _mockedNotifyClientFactory = null!;
        private Mock<INotifyClient> _mockedNotifyClient = null!;
        private Mock<ISerializationService> _mockedSerializer = null!;
        private Mock<IServiceProvider> _mockedServiceProvider = null!;

        private IMessageBoxScenario _scenario = null!;

        [SetUp]
        public void InitializeTests()
        {
            this._mockedDataQuery = new Mock<IDataQueryService<NotificationEvent>>(MockBehavior.Strict);
            this._mockedQueryContext = new Mock<IQueryContext>(MockBehavior.Strict);
            this._mockedNotifyClientFactory = new Mock<IHttpClientFactory<INotifyClient, string>>(MockBehavior.Strict);
            this._mockedNotifyClient = new Mock<INotifyClient>(MockBehavior.Strict);
            this._mockedSerializer = new Mock<ISerializationService>(MockBehavior.Strict);

            // BRP is treated as "not configured for this environment" by default - BrpClient's real
            // constructor requires live HTTP/Keycloak dependencies that aren't worth faking here, so every
            // test that reaches the letter fallback exercises the (real, supported) "BRP unavailable" path.
            this._mockedServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            this._mockedServiceProvider.Setup(mock => mock.GetService(typeof(BrpClient))).Returns(null!);

            this._mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            this._mockedSerializer
                .Setup(mock => mock.Serialize(It.IsAny<MessageBoxNotifyReference>()))
                .Returns("test-reference");

            this._mockedNotifyClientFactory
                .Setup(mock => mock.GetHttpClient(It.IsAny<string>()))
                .Returns(this._mockedNotifyClient.Object);

            using OmcConfiguration configuration = ConfigurationHandler.GetOmcConfigurationWith(
                ConfigurationHandler.TestLoaderTypesSetup.ValidEnvironment_v2);

            this._scenario = new MessageBoxScenarioImplementation(
                this._mockedDataQuery.Object,
                this._mockedNotifyClientFactory.Object,
                this._mockedSerializer.Object,
                configuration,
                this._mockedServiceProvider.Object,
                NullLogger<MessageBoxScenarioImplementation>.Instance);
        }

        #region Test data
        private static readonly Guid s_messageUuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private const string TestBsn = "123456789";
        private const string TestEmailAddress = "citizen@example.com";
        private static readonly Uri s_partyUri = new("https://openklant.test/partijen/22222222-2222-2222-2222-222222222222");

        private static JsonElement BuildCloudEvent(object? subject)
            => JsonSerializer.SerializeToElement(new { subject });

        private static VtbMessage BuildVtbMessage(
            bool isInMobbInbox = true,
            string messageType = "allowed-type",
            string messageText = "Hello citizen",
            string? recipientUrn = null,
            VtbMessage.Attachment[]? attachments = null)
        {
            return new VtbMessage
            {
                MessageText = messageText,
                RecipientUrn = recipientUrn ?? $"urn:nld:bsn:{TestBsn}",
                MessageType = messageType,
                IsInMyGovernmentMessageBox = isInMobbInbox,
                Attachments = attachments
            };
        }

        private static CommonPartyData BuildPartyData(string emailAddress = TestEmailAddress)
        {
            return new CommonPartyData
            {
                Uri = s_partyUri,
                Name = "Jane",
                SurnamePrefix = "van",
                Surname = "Doe",
                EmailAddress = emailAddress
            };
        }

        private static VtbMessage.Attachment BuildAttachment(Guid documentUuid, bool isMessageTypeAttachment = false)
        {
            return new VtbMessage.Attachment
            {
                InformationObjectUrn = $"urn:nld:informatieobject:uuid:{documentUuid}",
                IsMessageTypeAttachment = isMessageTypeAttachment
            };
        }

        private void SetUpVtbMessage(VtbMessage message)
        {
            this._mockedQueryContext
                .Setup(mock => mock.GetVtbMessageAsync(It.IsAny<Uri>()))
                .ReturnsAsync(message);
        }

        private void SetUpPartyData(CommonPartyData partyData)
        {
            this._mockedQueryContext
                .Setup(mock => mock.GetPartyDataAsync(null, TestBsn, null))
                .ReturnsAsync(partyData);
        }
        #endregion

        #region ProcessCloudEventAsync() - pre-send validation
        [Test]
        public async Task ProcessCloudEventAsync_MissingSubject_ReturnsFailure()
        {
            // Arrange
            JsonElement cloudEventWithoutSubject = JsonSerializer.SerializeToElement(new { });

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(cloudEventWithoutSubject);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("subject"));
            });
        }

        [Test]
        public async Task ProcessCloudEventAsync_InvalidUuidSubject_ReturnsFailure()
        {
            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent("not-a-guid"));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("Invalid UUID"));
            });
        }

        [Test]
        public async Task ProcessCloudEventAsync_EmptyMessageText_ReturnsFailure()
        {
            // Arrange
            SetUpVtbMessage(BuildVtbMessage(messageText: "   "));

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("Message text"));
            });
        }

        [Test]
        public async Task ProcessCloudEventAsync_NoBsnInRecipientUrn_ReturnsFailure()
        {
            // Arrange
            SetUpVtbMessage(BuildVtbMessage(recipientUrn: "urn:nld:no-bsn-here"));

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("BSN"));
            });
        }

        [Test]
        public async Task ProcessCloudEventAsync_NullMessageType_RejectedByWhitelistGate_ReturnsFailure()
        {
            // Arrange
            SetUpVtbMessage(BuildVtbMessage(messageType: null!));
            SetUpPartyData(BuildPartyData());

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("whitelist"));
            });
        }
        #endregion

        #region ProcessCloudEventAsync() - not eligible for MOBB inbox
        [Test]
        public async Task ProcessCloudEventAsync_NotInMobbInbox_WithEmail_SendsDigitalePost_ReturnsSuccess()
        {
            // Arrange
            SetUpVtbMessage(BuildVtbMessage(isInMobbInbox: false));
            SetUpPartyData(BuildPartyData(TestEmailAddress));

            this._mockedNotifyClient
                .Setup(mock => mock.SendEmailAsync(TestEmailAddress, It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Success());

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("Digitale post"));

                this._mockedNotifyClient.Verify(mock => mock.SendEmailAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()), Times.Once);
                this._mockedNotifyClient.Verify(mock => mock.SendMessageBoxNotificationAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<Attachment>>(), It.IsAny<string>()), Times.Never);
            });
        }

        [Test]
        public async Task ProcessCloudEventAsync_NotInMobbInbox_NoEmail_FallsToLetterFallback_ReturnsFailure()
        {
            // Arrange
            SetUpVtbMessage(BuildVtbMessage(isInMobbInbox: false));
            SetUpPartyData(BuildPartyData(emailAddress: string.Empty));

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert - BRP is "unavailable" in this test setup, so the letter fallback fails at BRP resolution
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("BRP"));

                this._mockedNotifyClient.Verify(mock => mock.SendEmailAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()), Times.Never);
            });
        }
        #endregion

        #region ProcessCloudEventAsync() - MOBB eligible
        [Test]
        public async Task ProcessCloudEventAsync_MobbEligible_NoUsableAttachments_ReturnsFailure_WithoutFallback()
        {
            // Arrange: the only attachment present is a standard pre-uploaded one, which is always skipped
            SetUpVtbMessage(BuildVtbMessage(attachments: [BuildAttachment(Guid.NewGuid(), isMessageTypeAttachment: true)]));
            SetUpPartyData(BuildPartyData());

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("attachments"));

                this._mockedNotifyClientFactory.Verify(mock => mock.GetHttpClient(It.IsAny<string>()), Times.Never);
            });
        }

        [Test]
        public async Task ProcessCloudEventAsync_MobbEligible_SendSucceeds_ReturnsSuccess()
        {
            // Arrange
            Guid documentUuid = Guid.NewGuid();
            SetUpVtbMessage(BuildVtbMessage(attachments: [BuildAttachment(documentUuid)]));
            SetUpPartyData(BuildPartyData());

            this._mockedQueryContext
                .Setup(mock => mock.GetDocumentAsync(documentUuid))
                .ReturnsAsync(new SingularInformationObject { Content = "base64content", Filename = "file.pdf" });

            this._mockedNotifyClient
                .Setup(mock => mock.SendMessageBoxNotificationAsync(
                    TestBsn, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<Attachment>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Success());

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public async Task ProcessCloudEventAsync_MobbEligible_SendFailsSynchronously_FallsBackToEmail_ReturnsSuccess()
        {
            // Arrange
            Guid documentUuid = Guid.NewGuid();
            SetUpVtbMessage(BuildVtbMessage(attachments: [BuildAttachment(documentUuid)]));
            SetUpPartyData(BuildPartyData(TestEmailAddress));

            this._mockedQueryContext
                .Setup(mock => mock.GetDocumentAsync(documentUuid))
                .ReturnsAsync(new SingularInformationObject { Content = "base64content", Filename = "file.pdf" });

            this._mockedNotifyClient
                .Setup(mock => mock.SendMessageBoxNotificationAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<Attachment>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Failure("MOBB rejected synchronously"));

            this._mockedNotifyClient
                .Setup(mock => mock.SendEmailAsync(TestEmailAddress, It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Success());

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("Digitale post"));

                // NOTE: no contactmoment is created for this synchronous rejection today (see project notes) -
                // this test only verifies the fallback itself is triggered.
                this._mockedNotifyClient.Verify(mock => mock.SendMessageBoxNotificationAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<Attachment>>(), It.IsAny<string>()), Times.Once);
                this._mockedNotifyClient.Verify(mock => mock.SendEmailAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()), Times.Once);
            });
        }

        [Test]
        public async Task ProcessCloudEventAsync_MobbAndEmailBothFailSynchronously_FallsToLetterFallback_ReturnsFailure()
        {
            // Arrange
            Guid documentUuid = Guid.NewGuid();
            SetUpVtbMessage(BuildVtbMessage(attachments: [BuildAttachment(documentUuid)]));
            SetUpPartyData(BuildPartyData(TestEmailAddress));

            this._mockedQueryContext
                .Setup(mock => mock.GetDocumentAsync(documentUuid))
                .ReturnsAsync(new SingularInformationObject { Content = "base64content", Filename = "file.pdf" });

            this._mockedNotifyClient
                .Setup(mock => mock.SendMessageBoxNotificationAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<Attachment>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Failure("MOBB rejected synchronously"));

            this._mockedNotifyClient
                .Setup(mock => mock.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Failure("Email rejected synchronously"));

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert - BRP unavailable in this test setup, so the letter fallback fails there
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("BRP"));
            });
        }
        #endregion

        #region HandleDeliveryFailureAsync()
        [Test]
        public async Task HandleDeliveryFailureAsync_LetterChannel_ReturnsTerminalFailure_WithoutReResolving()
        {
            // Arrange
            var reference = new MessageBoxNotifyReference { MessageId = s_messageUuid, PartyId = Guid.NewGuid(), Mobb = false, WasGefaaldeNotificatie = true };

            // Act
            HttpRequestResponse result = await this._scenario.HandleDeliveryFailureAsync(reference, NotifyMethods.Letter);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("no further fallback"));

                this._mockedDataQuery.Verify(mock => mock.From(It.IsAny<NotificationEvent>()), Times.Never);
            });
        }

        [TestCase(NotifyMethods.Sms)]
        [TestCase(NotifyMethods.None)]
        public async Task HandleDeliveryFailureAsync_UnrecognizedChannel_ReturnsFailure_WithoutReResolving(NotifyMethods failedChannel)
        {
            // Arrange
            var reference = new MessageBoxNotifyReference { MessageId = s_messageUuid, PartyId = Guid.NewGuid() };

            // Act
            HttpRequestResponse result = await this._scenario.HandleDeliveryFailureAsync(reference, failedChannel);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("unrecognized"));

                this._mockedDataQuery.Verify(mock => mock.From(It.IsAny<NotificationEvent>()), Times.Never);
            });
        }

        [Test]
        public async Task HandleDeliveryFailureAsync_MobbChannel_ReResolvesRecipient_FallsBackToEmail_ReturnsSuccess()
        {
            // Arrange
            SetUpVtbMessage(BuildVtbMessage());
            SetUpPartyData(BuildPartyData(TestEmailAddress));

            this._mockedNotifyClient
                .Setup(mock => mock.SendEmailAsync(TestEmailAddress, It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Success());

            var reference = new MessageBoxNotifyReference { MessageId = s_messageUuid, PartyId = Guid.NewGuid(), Mobb = true };

            // Act
            HttpRequestResponse result = await this._scenario.HandleDeliveryFailureAsync(reference, NotifyMethods.Mobb);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("Digitale post"));
            });
        }

        [Test]
        public async Task HandleDeliveryFailureAsync_EmailChannel_ReResolvesRecipient_FallsBackToLetterFallback_ReturnsFailure()
        {
            // Arrange
            SetUpVtbMessage(BuildVtbMessage());
            SetUpPartyData(BuildPartyData());

            var reference = new MessageBoxNotifyReference { MessageId = s_messageUuid, PartyId = Guid.NewGuid(), Mobb = false };

            // Act
            HttpRequestResponse result = await this._scenario.HandleDeliveryFailureAsync(reference, NotifyMethods.Email);

            // Assert - BRP unavailable in this test setup, so the letter fallback fails there
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("BRP"));
            });
        }

        [Test]
        public async Task HandleDeliveryFailureAsync_RecipientReResolutionThrows_ReturnsFailure()
        {
            // Arrange
            this._mockedQueryContext
                .Setup(mock => mock.GetVtbMessageAsync(It.IsAny<Uri>()))
                .ThrowsAsync(new HttpRequestException("OpenVTB unavailable"));

            var reference = new MessageBoxNotifyReference { MessageId = s_messageUuid, PartyId = Guid.NewGuid(), Mobb = true };

            // Act
            HttpRequestResponse result = await this._scenario.HandleDeliveryFailureAsync(reference, NotifyMethods.Mobb);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("re-resolve recipient context"));
            });
        }
        #endregion
    }
}
