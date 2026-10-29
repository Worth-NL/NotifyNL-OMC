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

            // Nothing was sent before, unless a test says otherwise
            this._mockedNotifyClient
                .Setup(mock => mock.FindNotificationByReferenceAsync(It.IsAny<string>(), It.IsAny<NotifyMethods>()))
                .ReturnsAsync(NotifyLookupResponse.NotFound());

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

        private const string BerichtGepubliceerdType = "nl.overheid.berichten.bericht-gepubliceerd";

        private static JsonElement BuildCloudEvent(object? subject, string type = BerichtGepubliceerdType)
            => JsonSerializer.SerializeToElement(new { type, subject });

        private static readonly Uri s_messageTypeUri = new("https://openvtb.test/berichten/api/v1/berichttypen/33333333-3333-3333-3333-333333333333");

        /// <summary>
        /// A Bericht and the BerichtType it links to: since Open VTB 0.2.0 the Berichtenbox code and eligibility
        /// live on the type, so a test describing "a MOBB-eligible message" needs both.
        /// </summary>
        private readonly record struct TestBericht(VtbMessage Message, VtbMessageType MessageType);

        private static TestBericht BuildVtbMessage(
            bool isInMobbInbox = true,
            string messageType = "allowed-type",
            string messageText = "Hello citizen",
            string? recipientUrn = null,
            VtbMessage.Attachment[]? attachments = null,
            Uri? messageTypeUri = null)
        {
            return new TestBericht(
                new VtbMessage
                {
                    MessageText = messageText,
                    RecipientUrn = recipientUrn ?? $"urn:nld:bsn:{TestBsn}",
                    MessageTypeUri = messageTypeUri ?? s_messageTypeUri,
                    Attachments = attachments
                },
                new VtbMessageType
                {
                    Url = s_messageTypeUri,
                    MessageBoxType = messageType,
                    IsInMyGovernmentMessageBox = isInMobbInbox
                });
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

        private static VtbMessage.Attachment BuildAttachment(Guid documentUuid)
        {
            return new VtbMessage.Attachment
            {
                InformationObjectUrn = $"urn:nld:informatieobject:uuid:{documentUuid}"
            };
        }

        private void SetUpVtbMessage(TestBericht bericht)
        {
            this._mockedQueryContext
                .Setup(mock => mock.GetVtbMessageAsync(It.IsAny<Uri>()))
                .ReturnsAsync(bericht.Message);

            this._mockedQueryContext
                .Setup(mock => mock.GetVtbMessageTypeAsync(s_messageTypeUri))
                .ReturnsAsync(bericht.MessageType);
        }

        private void SetUpPartyData(CommonPartyData partyData)
        {
            this._mockedQueryContext
                .Setup(mock => mock.GetPartyDataAsync(null, TestBsn, null, false, true))
                .ReturnsAsync(partyData);
        }
        #endregion

        #region ProcessCloudEventAsync() - CloudEvent type gating
        [Test]
        public async Task ProcessCloudEventAsync_BerichtGeregistreerdType_ReturnsSuccess_WithoutProcessing()
        {
            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(
                BuildCloudEvent(s_messageUuid, type: "nl.overheid.berichten.bericht-geregistreerd"));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("does not require action"));
            });

            // No VtbMessage lookup, no party lookup, no send should have happened
            this._mockedQueryContext.Verify(mock => mock.GetVtbMessageAsync(It.IsAny<Uri>()), Times.Never);
        }

        [Test]
        public async Task ProcessCloudEventAsync_UnrecognizedType_ReturnsSuccess_WithoutProcessing()
        {
            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(
                BuildCloudEvent(s_messageUuid, type: "nl.overheid.berichten.some-future-type"));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("does not require action"));
            });

            this._mockedQueryContext.Verify(mock => mock.GetVtbMessageAsync(It.IsAny<Uri>()), Times.Never);
        }

        [Test]
        public async Task ProcessCloudEventAsync_MissingType_ReturnsSuccess_WithoutProcessing()
        {
            // Arrange
            JsonElement cloudEventWithoutType = JsonSerializer.SerializeToElement(new { subject = s_messageUuid });

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(cloudEventWithoutType);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("does not require action"));
            });

            this._mockedQueryContext.Verify(mock => mock.GetVtbMessageAsync(It.IsAny<Uri>()), Times.Never);
        }
        #endregion

        #region ProcessCloudEventAsync() - pre-send validation
        [Test]
        public async Task ProcessCloudEventAsync_MissingSubject_ReturnsFailure()
        {
            // Arrange
            JsonElement cloudEventWithoutSubject = JsonSerializer.SerializeToElement(new { type = BerichtGepubliceerdType });

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
        public async Task ProcessCloudEventAsync_EmptyMessageText_IsDropped_ReturnsSuccess()
        {
            // Arrange
            SetUpVtbMessage(BuildVtbMessage(messageText: "   "));

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("Dropped"));
                Assert.That(result.JsonResponse, Does.Contain("message text"));
            });
        }

        [Test]
        public async Task ProcessCloudEventAsync_NoBsnInRecipientUrn_IsDropped_ReturnsSuccess()
        {
            // Arrange
            SetUpVtbMessage(BuildVtbMessage(recipientUrn: "urn:nld:no-bsn-here"));

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("Dropped"));
                Assert.That(result.JsonResponse, Does.Contain("BSN"));
            });
        }

        [Test]
        public async Task ProcessCloudEventAsync_NullMessageType_RejectedByWhitelistGate_IsDropped_ReturnsSuccess()
        {
            // Arrange
            SetUpVtbMessage(BuildVtbMessage(messageType: null!));
            SetUpPartyData(BuildPartyData());

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("Dropped"));
                Assert.That(result.JsonResponse, Does.Contain("whitelist"));
            });
        }

        [Test]
        public async Task ProcessCloudEventAsync_NoMessageType_IsDropped_ReturnsSuccess()
        {
            // Arrange
            SetUpVtbMessage(BuildVtbMessage(messageTypeUri: Common.Constants.CommonValues.Default.Models.EmptyUri));

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("no berichtType"));

                this._mockedQueryContext.Verify(mock => mock.GetVtbMessageTypeAsync(It.IsAny<Uri>()), Times.Never);
            });
        }

        [Test]
        public void ProcessCloudEventAsync_MessageTypeLookupFails_Throws_SoOpenVtbRedelivers()
        {
            // Arrange
            this._mockedQueryContext
                .Setup(mock => mock.GetVtbMessageAsync(It.IsAny<Uri>()))
                .ReturnsAsync(BuildVtbMessage().Message);
            this._mockedQueryContext
                .Setup(mock => mock.GetVtbMessageTypeAsync(s_messageTypeUri))
                .ThrowsAsync(new HttpRequestException("OpenVTB unavailable"));

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(() => this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid)));
        }

        [Test]
        public async Task ProcessCloudEventAsync_MobbEligible_ReferenceCarriesTheBerichtType_ForTheKlantcontactsHoofdOnderwerpType()
        {
            // Arrange
            SetUpVtbMessage(BuildVtbMessage(attachments: []));
            SetUpPartyData(BuildPartyData());

            MessageBoxNotifyReference? serializedReference = null;
            this._mockedSerializer
                .Setup(mock => mock.Serialize(It.IsAny<MessageBoxNotifyReference>()))
                .Callback<MessageBoxNotifyReference>(reference => serializedReference = reference)
                .Returns("test-reference");

            this._mockedNotifyClient
                .Setup(mock => mock.SendMessageBoxNotificationAsync(
                    TestBsn, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<Attachment>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Success());

            // Act
            await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert
            Assert.That(serializedReference?.MessageTypeUri, Is.EqualTo(s_messageTypeUri));
        }

        #region Never sending the same Bericht twice (#247)
        [Test]
        public async Task ProcessCloudEventAsync_MobbAlreadySent_DoesNotSendItAgain()
        {
            // Arrange: Open Notificaties delivers the same Bericht a second time
            SetUpVtbMessage(BuildVtbMessage(attachments: []));
            SetUpPartyData(BuildPartyData());

            this._mockedNotifyClient
                .Setup(mock => mock.FindNotificationByReferenceAsync(It.IsAny<string>(), It.IsAny<NotifyMethods>()))
                .ReturnsAsync(NotifyLookupResponse.Found("44444444-4444-4444-4444-444444444444", "delivered"));

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("Already sent"));

                this._mockedNotifyClient.Verify(mock => mock.SendMessageBoxNotificationAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<Attachment>>(), It.IsAny<string>()), Times.Never);
            });
        }

        [Test]
        public async Task ProcessCloudEventAsync_LookupFails_DoesNotSendBlindly()
        {
            // Arrange
            SetUpVtbMessage(BuildVtbMessage(attachments: []));
            SetUpPartyData(BuildPartyData());

            this._mockedNotifyClient
                .Setup(mock => mock.FindNotificationByReferenceAsync(It.IsAny<string>(), It.IsAny<NotifyMethods>()))
                .ReturnsAsync(NotifyLookupResponse.Failure("Notify NL unavailable"));

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert: a failure, so Open Notificaties redelivers once Notify NL can be asked again
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);

                this._mockedNotifyClient.Verify(mock => mock.SendMessageBoxNotificationAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<Attachment>>(), It.IsAny<string>()), Times.Never);
            });
        }

        [Test]
        public async Task HandleDeliveryFailureAsync_EmailFallbackAlreadySent_DoesNotSendItAgain_AndReportsItAsSent()
        {
            // Arrange: the MOBB failure callback arrives a second time, e.g. because registering its contactmoment failed
            SetUpVtbMessage(BuildVtbMessage());
            SetUpPartyData(BuildPartyData(TestEmailAddress));

            this._mockedNotifyClient
                .Setup(mock => mock.FindNotificationByReferenceAsync(It.IsAny<string>(), It.IsAny<NotifyMethods>()))
                .ReturnsAsync(NotifyLookupResponse.Found("55555555-5555-5555-5555-555555555555", "sending"));

            var reference = new MessageBoxNotifyReference { MessageId = s_messageUuid, PartyId = Guid.NewGuid(), Mobb = true };

            // Act
            HttpRequestResponse result = await this._scenario.HandleDeliveryFailureAsync(reference, NotifyMethods.Mobb);

            // Assert: reported as sent, so the contactmoment still says further action was taken
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("Already sent"));

                this._mockedNotifyClient.Verify(mock => mock.SendEmailAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()), Times.Never);
            });
        }

        [Test]
        public async Task HandleDeliveryFailureAsync_EmailFallbackNotSentBefore_SendsIt_WithTheReferenceItLookedUp()
        {
            // Arrange
            SetUpVtbMessage(BuildVtbMessage());
            SetUpPartyData(BuildPartyData(TestEmailAddress));

            string? lookedUpReference = null;
            this._mockedNotifyClient
                .Setup(mock => mock.FindNotificationByReferenceAsync(It.IsAny<string>(), It.IsAny<NotifyMethods>()))
                .Callback<string, NotifyMethods>((lookedUp, _) => lookedUpReference = lookedUp)
                .ReturnsAsync(NotifyLookupResponse.NotFound());

            string? sentReference = null;
            this._mockedNotifyClient
                .Setup(mock => mock.SendEmailAsync(TestEmailAddress, It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .Callback<string, string, Dictionary<string, object>, string>((_, _, _, sent) => sentReference = sent)
                .ReturnsAsync(NotifySendResponse.Success());

            var reference = new MessageBoxNotifyReference { MessageId = s_messageUuid, PartyId = Guid.NewGuid(), Mobb = true };

            // Act
            HttpRequestResponse result = await this._scenario.HandleDeliveryFailureAsync(reference, NotifyMethods.Mobb);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);

                Assert.That(sentReference, Is.Not.Null.And.EqualTo(lookedUpReference));  // checked for exactly what it sends

                this._mockedNotifyClient.Verify(mock => mock.SendEmailAsync(
                    TestEmailAddress, It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()), Times.Once);
            });
        }
        #endregion

        [Test]
        public async Task ProcessCloudEventAsync_MobbEligible_SendsTheBerichtTypeCode_NotItsUrl()
        {
            // Arrange: Logius knows the message by the BerichtType's 8-character code; the Bericht itself only links to it
            SetUpVtbMessage(BuildVtbMessage(messageType: "MOBBCODE", attachments: []));
            SetUpPartyData(BuildPartyData());

            this._mockedNotifyClient
                .Setup(mock => mock.SendMessageBoxNotificationAsync(
                    TestBsn, It.IsAny<string>(), "MOBBCODE", It.IsAny<string>(), It.IsAny<IEnumerable<Attachment>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Success());

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);

                this._mockedNotifyClient.Verify(mock => mock.SendMessageBoxNotificationAsync(
                    TestBsn, It.IsAny<string>(), "MOBBCODE", It.IsAny<string>(), It.IsAny<IEnumerable<Attachment>>(), It.IsAny<string>()), Times.Once);
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
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<Attachment>>(), It.IsAny<string>()), Times.Never);
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
        public async Task ProcessCloudEventAsync_MobbEligible_NoUsableAttachments_SendsWithEmptyAttachments_ReturnsSuccess()
        {
            // Arrange: 0 to 2 attachments are allowed, so a message without any should still proceed to a MOBB
            // send (matching the underlying GovukNotify client's own relaxed validation), not be dropped.
            SetUpVtbMessage(BuildVtbMessage(attachments: []));
            SetUpPartyData(BuildPartyData());

            this._mockedNotifyClient
                .Setup(mock => mock.SendMessageBoxNotificationAsync(
                    TestBsn, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.Is<IEnumerable<Attachment>>(attachments => !attachments.Any()), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Success());

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);

                this._mockedNotifyClient.Verify(mock => mock.SendMessageBoxNotificationAsync(
                    TestBsn, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.Is<IEnumerable<Attachment>>(attachments => !attachments.Any()), It.IsAny<string>()), Times.Once);
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
                    TestBsn, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<Attachment>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Success());

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public async Task ProcessCloudEventAsync_MobbEligible_AttachmentContentIsResolved_NotTheRawDownloadUri()
        {
            // Arrange - "inhoud" is a download URI on GET, not the file content itself (confirmed against
            // the live Documenten API), so GetDocumentAsync alone must not be treated as usable attachment
            // content - the actual bytes need a second, explicit GetDocumentContentAsync fetch.
            Guid documentUuid = Guid.NewGuid();
            SetUpVtbMessage(BuildVtbMessage(attachments: [BuildAttachment(documentUuid)]));
            SetUpPartyData(BuildPartyData());

            Uri downloadUri = new("https://openzaak.test/documenten/api/v1/enkelvoudiginformatieobjecten/" +
                                   $"{documentUuid:D}/download?versie=1");
            const string realBase64Content = "cmVhbC1wZGYtYnl0ZXM=";  // NOT the download URI

            this._mockedQueryContext
                .Setup(mock => mock.GetDocumentAsync(documentUuid))
                .ReturnsAsync(new SingularInformationObject { Content = downloadUri.ToString(), Filename = "file.pdf" });

            this._mockedQueryContext
                .Setup(mock => mock.GetDocumentContentAsync(downloadUri))
                .ReturnsAsync(realBase64Content);

            this._mockedNotifyClient
                .Setup(mock => mock.SendMessageBoxNotificationAsync(
                    TestBsn, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.Is<IEnumerable<Attachment>>(attachments =>
                        attachments.Single().file == realBase64Content &&
                        attachments.Single().file != downloadUri.ToString()),
                    It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Success());

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);

                this._mockedQueryContext.Verify(mock => mock.GetDocumentContentAsync(downloadUri), Times.Once);
                this._mockedNotifyClient.Verify(mock => mock.SendMessageBoxNotificationAsync(
                    TestBsn, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.Is<IEnumerable<Attachment>>(attachments => attachments.Single().file == realBase64Content),
                    It.IsAny<string>()), Times.Once);
            });
        }

        [Test]
        public async Task ProcessCloudEventAsync_MobbEligible_AttachmentContentFetchFails_SkipsThatAttachment_StillSucceeds()
        {
            // Arrange - a failure resolving the real content (e.g. Documenten API unreachable) must not
            // break the whole send; the attachment is just dropped, matching the existing per-attachment
            // failure handling for a bad UUID/URN.
            Guid documentUuid = Guid.NewGuid();
            SetUpVtbMessage(BuildVtbMessage(attachments: [BuildAttachment(documentUuid)]));
            SetUpPartyData(BuildPartyData());

            this._mockedQueryContext
                .Setup(mock => mock.GetDocumentAsync(documentUuid))
                .ReturnsAsync(new SingularInformationObject { Content = "https://openzaak.test/download", Filename = "file.pdf" });

            this._mockedQueryContext
                .Setup(mock => mock.GetDocumentContentAsync(It.IsAny<Uri>()))
                .ThrowsAsync(new HttpRequestException("Documenten API unreachable"));

            this._mockedNotifyClient
                .Setup(mock => mock.SendMessageBoxNotificationAsync(
                    TestBsn, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.Is<IEnumerable<Attachment>>(attachments => !attachments.Any()), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Success());

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public async Task ProcessCloudEventAsync_MobbEligible_NotAcceptedByNotify_ReturnsFailure_WithoutFallingBack()
        {
            // NOTE: Notify NL not accepting the send says nothing about the citizen's Berichtenbox (that only comes
            //       later, from the callback), so no e-mail or letter: Open Notificaties delivers again (#243).

            // Arrange
            Guid documentUuid = Guid.NewGuid();
            SetUpVtbMessage(BuildVtbMessage(attachments: [BuildAttachment(documentUuid)]));
            SetUpPartyData(BuildPartyData(TestEmailAddress));

            this._mockedQueryContext
                .Setup(mock => mock.GetDocumentAsync(documentUuid))
                .ReturnsAsync(new SingularInformationObject { Content = "base64content", Filename = "file.pdf" });

            this._mockedNotifyClient
                .Setup(mock => mock.SendMessageBoxNotificationAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<Attachment>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Failure("503 Service Unavailable"));

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("not accepted by Notify NL"));

                this._mockedNotifyClient.Verify(mock => mock.SendEmailAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()), Times.Never);
                this._mockedNotifyClient.Verify(mock => mock.SendLetterAsync(
                    It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>?>()), Times.Never);
            });
        }

        [Test]
        public async Task ProcessCloudEventAsync_NotInMobbInbox_EmailNotAcceptedByNotify_ReturnsFailure_WithoutLetter()
        {
            // NOTE: A technical problem at Notify NL must not cost the citizen a paper letter (#243).

            // Arrange
            SetUpVtbMessage(BuildVtbMessage(isInMobbInbox: false));
            SetUpPartyData(BuildPartyData(TestEmailAddress));

            this._mockedNotifyClient
                .Setup(mock => mock.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Failure("429 Too Many Requests"));

            // Act
            HttpRequestResponse result = await this._scenario.ProcessCloudEventAsync(BuildCloudEvent(s_messageUuid));

            // Assert: BRP is unavailable in this setup, so mentioning it would mean the letter was tried
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.JsonResponse, Does.Contain("not accepted by Notify NL"));
                Assert.That(result.JsonResponse, Does.Not.Contain("BRP"));
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
        public async Task HandleDeliveryFailureAsync_MobbChannel_EmailNotAcceptedByNotify_StillFallsBackToLetter()
        {
            // NOTE: On the callback path nothing delivers this again, so the letter rather than nothing (#243).

            // Arrange
            SetUpVtbMessage(BuildVtbMessage());
            SetUpPartyData(BuildPartyData(TestEmailAddress));

            this._mockedNotifyClient
                .Setup(mock => mock.SendEmailAsync(TestEmailAddress, It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Failure("503 Service Unavailable"));

            var reference = new MessageBoxNotifyReference { MessageId = s_messageUuid, PartyId = Guid.NewGuid(), Mobb = true };

            // Act
            HttpRequestResponse result = await this._scenario.HandleDeliveryFailureAsync(reference, NotifyMethods.Mobb);

            // Assert: BRP is unavailable in this setup, so the letter fails there - but it was tried
            Assert.That(result.JsonResponse, Does.Contain("BRP"));
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
