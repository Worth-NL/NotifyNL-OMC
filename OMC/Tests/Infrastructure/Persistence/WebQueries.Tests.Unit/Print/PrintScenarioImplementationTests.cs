// © 2026, Worth Systems.

using System.Text;
using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Clients.Factories.Interfaces;
using WebQueries.DataSending.Clients.Interfaces;
using WebQueries.DataSending.Models.Reponses;
using WebQueries.Print;
using WebQueries.Print.Interfaces;
using WebQueries.Print.Models;
using ZgwModels.Serialization.Interfaces;
using WebQueries.Register.Interfaces;
using ZgwModels.Enums;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.Objecten.Print;
using ZgwModels.Mapping.Models.POCOs.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.OpenZaak.Documents;

namespace WebQueries.Tests.Unit.Print
{
    [TestFixture]
    public sealed class PrintScenarioImplementationTests
    {
        private Mock<IDataQueryService<NotificationEvent>> _mockedDataQuery = null!;
        private Mock<IQueryContext> _mockedQueryContext = null!;
        private Mock<IHttpClientFactory<INotifyClient, string>> _mockedNotifyClientFactory = null!;
        private Mock<INotifyClient> _mockedNotifyClient = null!;
        private Mock<ITelemetryService> _mockedTelemetry = null!;
        private Mock<ISerializationService> _mockedSerializer = null!;

        private OmcConfiguration _configuration = null!;

        #region Test data
        private static readonly Guid s_objectId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid s_documentId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        private static readonly Uri s_partyUri = new("https://openklant.test/partijen/22222222-2222-2222-2222-222222222222");

        // Matches ZGW_ENDPOINT_DOCUMENTEN in the test configuration ("test.domain/api/v1").
        private static readonly Uri s_validPdfUri = new($"https://test.domain/api/v1/enkelvoudiginformatieobjecten/{s_documentId}");
        private static readonly Uri s_contentUri = new("https://test.domain/api/v1/enkelvoudiginformatieobjecten/download");

        private const string TestBsn = "123456782";
        private const string TestBsnUrn = $"urn:nld:bsn:nummer:{TestBsn}";
        private const string TestSubject = "Wij hebben aanvullende informatie nodig";

        private static readonly byte[] s_pdfBytes = Encoding.UTF8.GetBytes("%PDF-1.7 test");
        #endregion

        [SetUp]
        public void InitializeTests()
        {
            this._mockedDataQuery = new Mock<IDataQueryService<NotificationEvent>>(MockBehavior.Strict);
            this._mockedQueryContext = new Mock<IQueryContext>(MockBehavior.Strict);
            this._mockedNotifyClientFactory = new Mock<IHttpClientFactory<INotifyClient, string>>(MockBehavior.Strict);
            this._mockedNotifyClient = new Mock<INotifyClient>(MockBehavior.Strict);
            this._mockedTelemetry = new Mock<ITelemetryService>(MockBehavior.Strict);
            this._mockedSerializer = new Mock<ISerializationService>(MockBehavior.Strict);
            this._mockedSerializer
                .Setup(mock => mock.Serialize(It.IsAny<PrintNotifyReference>()))
                .Returns("test-reference");

            this._mockedDataQuery
                .Setup(mock => mock.From(It.IsAny<NotificationEvent>()))
                .Returns(this._mockedQueryContext.Object);

            this._mockedNotifyClientFactory
                .Setup(mock => mock.GetHttpClient(It.IsAny<string>()))
                .Returns(this._mockedNotifyClient.Object);

            this._configuration = ConfigurationHandler.GetOmcConfigurationWith(
                ConfigurationHandler.TestLoaderTypesSetup.ValidEnvironment_v2);
        }

        [TearDown]
        public void CleanUpTests()
        {
            this._configuration.Dispose();
        }

        #region Whitelist
        [Test]
        public async Task ProcessPrintAsync_PrintNotAllowed_ReturnsFailureAndTouchesNothing()
        {
            // Arrange: "InvalidEnvironment" is how the other whitelist tests express a disabled flag - it
            // resolves ZGW_WHITELIST_PRINT_ALLOWED to "false" (see MessageReceivedScenarioTests).
            IPrintScenario scenario = GetScenario(ConfigurationHandler.TestLoaderTypesSetup.InvalidEnvironment_v2);

            // Act
            HttpRequestResponse actualResult = await scenario.ProcessPrintAsync(GetNotification());

            // Assert: nothing was fetched, sent or deleted.
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.JsonResponse, Does.Contain("ZGW_WHITELIST_PRINT_ALLOWED"));
            });

            this._mockedDataQuery.Verify(mock => mock.From(It.IsAny<NotificationEvent>()), Times.Never);
            this._mockedNotifyClient.Verify(
                mock => mock.SendPrecompiledLetterAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string?>()),
                Times.Never);
        }
        #endregion

        #region pdfurl validation
        // An entirely different host - the case the check exists for.
        [TestCase("https://evil.test/enkelvoudiginformatieobjecten/33333333-3333-3333-3333-333333333333")]
        // A lookalike that only contains the real domain inside its path, which a prefix check would miss.
        [TestCase("https://evil.test/https://test.domain/api/v1/enkelvoudiginformatieobjecten/33333333-3333-3333-3333-333333333333")]
        // Right host, wrong scheme.
        [TestCase("http://test.domain/api/v1/enkelvoudiginformatieobjecten/33333333-3333-3333-3333-333333333333")]
        // Right host and scheme, wrong port.
        [TestCase("https://test.domain:8443/api/v1/enkelvoudiginformatieobjecten/33333333-3333-3333-3333-333333333333")]
        public async Task ProcessPrintAsync_PdfUrlOutsideDocumentenApi_ReturnsFailureWithoutFetching(string testPdfUri)
        {
            // Arrange
            IPrintScenario scenario = GetAllowedScenario();
            SetupPrintObject(GetPrintData(pdfUri: new Uri(testPdfUri)));

            // Act
            HttpRequestResponse actualResult = await scenario.ProcessPrintAsync(GetNotification());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.JsonResponse, Does.Contain("refusing to fetch it").Or.Contain("not an absolute URI"));
            });

            this._mockedQueryContext.Verify(mock => mock.GetDocumentAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task ProcessPrintAsync_MissingPdfUrl_ReturnsFailure()
        {
            // Arrange
            IPrintScenario scenario = GetAllowedScenario();
            SetupPrintObject(GetPrintData(omitPdfUri: true));

            // Act
            HttpRequestResponse actualResult = await scenario.ProcessPrintAsync(GetNotification());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.JsonResponse, Does.Contain("no pdfurl"));
            });
        }
        #endregion

        #region contact_betrokkene_urn resolution
        [Test]
        public async Task ProcessPrintAsync_KvkUrn_ReturnsFailureNamingTheNamespace()
        {
            // Arrange: #237's own payload example is KVK-based, which cannot be resolved to a partij until
            // KVK party lookup lands (#171-#173, #205).
            IPrintScenario scenario = GetAllowedScenario();
            SetupPrintObject(GetPrintData(betrokkeneUrn: "urn:nld:hr:kvk:nummer:12345678"));

            // Act
            HttpRequestResponse actualResult = await scenario.ProcessPrintAsync(GetNotification());

            // Assert: explicit and visible, naming what it was handed - not silently dropped.
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.JsonResponse, Does.Contain("kvk"));
                Assert.That(actualResult.JsonResponse, Does.Contain("only BSN-based URNs"));
            });

            this._mockedQueryContext.Verify(
                mock => mock.GetPartyDataAsync(It.IsAny<Uri?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>()),
                Times.Never);
        }

        [TestCase("", "no contact_betrokkene_urn")]
        [TestCase("not-a-urn", "not a URN OMC can read")]
        [TestCase("urn:nld:bsn:nummer:12345", "not a nine-digit number")]
        public async Task ProcessPrintAsync_UnusableUrn_ReturnsFailure(string testUrn, string expectedReason)
        {
            // Arrange
            IPrintScenario scenario = GetAllowedScenario();
            SetupPrintObject(GetPrintData(betrokkeneUrn: testUrn));

            // Act
            HttpRequestResponse actualResult = await scenario.ProcessPrintAsync(GetNotification());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.JsonResponse, Does.Contain(expectedReason));
            });
        }

        [Test]
        public async Task ProcessPrintAsync_PartyLookupThrows_ReturnsFailureWithoutSending()
        {
            // Arrange
            IPrintScenario scenario = GetAllowedScenario();
            SetupPrintObject(GetPrintData());

            this._mockedQueryContext
                .Setup(mock => mock.GetPartyDataAsync(null, TestBsn, null, false))
                .ThrowsAsync(new HttpRequestException("Party not found"));

            // Act
            HttpRequestResponse actualResult = await scenario.ProcessPrintAsync(GetNotification());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.JsonResponse, Does.Contain("could not be resolved"));
            });

            this._mockedNotifyClient.Verify(
                mock => mock.SendPrecompiledLetterAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string?>()),
                Times.Never);
        }
        #endregion

        #region PDF download
        [Test]
        public async Task ProcessPrintAsync_EmptyDocumentContent_ReturnsFailureWithoutSending()
        {
            // Arrange
            IPrintScenario scenario = GetAllowedScenario();
            SetupPrintObject(GetPrintData());
            SetupParty();

            this._mockedQueryContext
                .Setup(mock => mock.GetDocumentAsync(s_documentId))
                .ReturnsAsync(new SingularInformationObject { Content = null });

            // Act
            HttpRequestResponse actualResult = await scenario.ProcessPrintAsync(GetNotification());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.JsonResponse, Does.Contain("no content to print"));
            });

            this._mockedNotifyClient.Verify(
                mock => mock.SendPrecompiledLetterAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string?>()),
                Times.Never);
        }
        #endregion

        #region Sending
        [Test]
        public async Task ProcessPrintAsync_NotifyRejectsLetter_ReturnsFailureAndKeepsObject()
        {
            // Arrange
            IPrintScenario scenario = GetAllowedScenario();
            SetupPrintObject(GetPrintData());
            SetupParty();
            SetupDocument();

            this._mockedNotifyClient
                .Setup(mock => mock.SendPrecompiledLetterAsync(It.IsAny<string>(), It.IsAny<byte[]>(), null))
                .ReturnsAsync(NotifySendResponse.Failure("Letters are not enabled for this service"));

            // Act
            HttpRequestResponse actualResult = await scenario.ProcessPrintAsync(GetNotification());

            // Assert: nothing is registered or deleted when the letter never left.
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.JsonResponse, Does.Contain("Precompiled letter rejected"));
            });

            this._mockedQueryContext.Verify(mock => mock.DeleteObjectAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task ProcessPrintAsync_HappyPath_SendsPdfButRegistersNothingYet()
        {
            // Arrange
            IPrintScenario scenario = GetAllowedScenario();
            SetupPrintObject(GetPrintData());
            SetupParty();
            SetupDocument();
            SetupSuccessfulSend();

            // Act
            HttpRequestResponse actualResult = await scenario.ProcessPrintAsync(GetNotification());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);

                // The PDF is forwarded as raw bytes, decoded from the Base64 the Documenten API hands back.
                this._mockedNotifyClient.Verify(
                    mock => mock.SendPrecompiledLetterAsync(
                        It.IsAny<string>(),
                        It.Is<byte[]>(bytes => bytes.SequenceEqual(s_pdfBytes)),
                        null),
                    Times.Once);

                // A 201 only means Notify accepted the request - the PDF is validated afterwards and can
                // still be rejected, so nothing is recorded and nothing is destroyed until the receipt.
                this._mockedTelemetry.Verify(mock => mock.ReportPrintCompletionAsync(
                    It.IsAny<PrintNotifyReference>(), It.IsAny<NotifyMethods>(), It.IsAny<string[]>()), Times.Never);
                this._mockedQueryContext.Verify(mock => mock.DeleteObjectAsync(It.IsAny<Guid>()), Times.Never);

                // A postal letter must not demand a digital address: a citizen with no e-mail or phone
                // number on file is exactly who a printed letter is for.
                this._mockedQueryContext.Verify(
                    mock => mock.GetPartyDataAsync(null, TestBsn, null, false), Times.Once);
            });
        }
        #endregion

        #region Delivery callback
        [Test]
        public async Task HandleDeliveryCallbackAsync_Delivered_RegistersContactMomentAndDeletesObject()
        {
            // Arrange
            IPrintScenario scenario = GetAllowedScenario();

            PrintNotifyReference capturedReference = default;
            string[] capturedMessages = [];
            this._mockedTelemetry
                .Setup(mock => mock.ReportPrintCompletionAsync(
                    It.IsAny<PrintNotifyReference>(), It.IsAny<NotifyMethods>(), It.IsAny<string[]>()))
                .Callback((PrintNotifyReference r, NotifyMethods _, string[] m) => { capturedReference = r; capturedMessages = m; })
                .ReturnsAsync(HttpRequestResponse.Success("registered"));

            this._mockedQueryContext
                .Setup(mock => mock.DeleteObjectAsync(s_objectId))
                .ReturnsAsync(HttpRequestResponse.Success("deleted"));

            // Act
            HttpRequestResponse actualResult = await scenario.HandleDeliveryCallbackAsync(
                GetReference(), NotifyMethods.Letter, succeeded: true, messages: ["subject", "body", "true", "sent-at"]);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);
                Assert.That(capturedReference.AttachmentId, Is.EqualTo(s_documentId));
                Assert.That(capturedMessages[2], Is.EqualTo("true"));
                this._mockedQueryContext.Verify(mock => mock.DeleteObjectAsync(s_objectId), Times.Once);
            });
        }

        [Test]
        public async Task HandleDeliveryCallbackAsync_Failed_RegistersFailureAndKeepsObject()
        {
            // Arrange
            IPrintScenario scenario = GetAllowedScenario();

            this._mockedTelemetry
                .Setup(mock => mock.ReportPrintCompletionAsync(
                    It.IsAny<PrintNotifyReference>(), It.IsAny<NotifyMethods>(), It.IsAny<string[]>()))
                .ReturnsAsync(HttpRequestResponse.Success("registered"));

            // Act
            HttpRequestResponse actualResult = await scenario.HandleDeliveryCallbackAsync(
                GetReference(), NotifyMethods.Letter, succeeded: false, messages: ["subject", "body", "false", ""]);

            // Assert: the object is the only record of what should have been printed, so a failed letter
            // leaves it in place to be inspected or retried.
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);
                Assert.That(actualResult.JsonResponse, Does.Contain("kept"));
                this._mockedQueryContext.Verify(mock => mock.DeleteObjectAsync(It.IsAny<Guid>()), Times.Never);
            });
        }

        [Test]
        public async Task HandleDeliveryCallbackAsync_ContactMomentFails_ReportsFailureAndKeepsObject()
        {
            // Arrange
            IPrintScenario scenario = GetAllowedScenario();

            this._mockedTelemetry
                .Setup(mock => mock.ReportPrintCompletionAsync(
                    It.IsAny<PrintNotifyReference>(), It.IsAny<NotifyMethods>(), It.IsAny<string[]>()))
                .ReturnsAsync(HttpRequestResponse.Failure("OpenKlant unavailable"));

            // Act
            HttpRequestResponse actualResult = await scenario.HandleDeliveryCallbackAsync(
                GetReference(), NotifyMethods.Letter, succeeded: true, messages: ["subject", "body", "true", "sent-at"]);

            // Assert: the object survives so the registration can be retried without reprinting.
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsFailure, Is.True);
                Assert.That(actualResult.JsonResponse, Does.Contain("Registering the print contactmoment failed"));
                this._mockedQueryContext.Verify(mock => mock.DeleteObjectAsync(It.IsAny<Guid>()), Times.Never);
            });
        }

        [Test]
        public async Task HandleDeliveryCallbackAsync_DeleteFails_StillReportsSuccess()
        {
            // Arrange
            IPrintScenario scenario = GetAllowedScenario();

            this._mockedTelemetry
                .Setup(mock => mock.ReportPrintCompletionAsync(
                    It.IsAny<PrintNotifyReference>(), It.IsAny<NotifyMethods>(), It.IsAny<string[]>()))
                .ReturnsAsync(HttpRequestResponse.Success("registered"));

            this._mockedQueryContext
                .Setup(mock => mock.DeleteObjectAsync(s_objectId))
                .ReturnsAsync(HttpRequestResponse.Failure("Objecten unavailable"));

            // Act
            HttpRequestResponse actualResult = await scenario.HandleDeliveryCallbackAsync(
                GetReference(), NotifyMethods.Letter, succeeded: true, messages: ["subject", "body", "true", "sent-at"]);

            // Assert: failing here would invite a retry that prints a second copy, so the undeleted object
            // is reported rather than treated as a failure.
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.IsSuccess, Is.True);
                Assert.That(actualResult.JsonResponse, Does.Contain("could not be deleted"));
            });
        }
        #endregion

        #region Helper methods
        private IPrintScenario GetScenario(ConfigurationHandler.TestLoaderTypesSetup setup)
        {
            this._configuration.Dispose();
            this._configuration = ConfigurationHandler.GetOmcConfigurationWith(setup);

            return new PrintScenarioImplementation(
                this._mockedDataQuery.Object,
                this._mockedNotifyClientFactory.Object,
                this._mockedTelemetry.Object,
                this._mockedSerializer.Object,
                this._configuration,
                NullLogger<PrintScenarioImplementation>.Instance);
        }

        /// <summary>
        /// A scenario whose configuration has printing switched on.
        /// </summary>
        private IPrintScenario GetAllowedScenario()
            => GetScenario(ConfigurationHandler.TestLoaderTypesSetup.ValidEnvironment_v2);

        private static NotificationEvent GetNotification()
        {
            return new NotificationEvent
            {
                MainObjectUri = new Uri($"https://objecten.test/objects/{s_objectId}")
            };
        }

        private static PrintData GetPrintData(
            Uri? pdfUri = null,
            string? betrokkeneUrn = null,
            bool includeSubjectObject = true,
            bool omitPdfUri = false)
        {
            return new PrintData
            {
                PdfUri = omitPdfUri ? null : pdfUri ?? s_validPdfUri,
                Subject = TestSubject,
                BetrokkeneUrn = betrokkeneUrn ?? TestBsnUrn,
                SubjectObjectIdentifier = includeSubjectObject
                    ? new SubjectObjectIdentifier
                    {
                        ObjectId = "44444444-4444-4444-4444-444444444444",
                        CodeObjectType = "zaak",
                        CodeRegister = "openzaak",
                        CodeSoortObjectId = "uuid"
                    }
                    : null
            };
        }

        private void SetupPrintObject(PrintData printData)
        {
            this._mockedQueryContext
                .Setup(mock => mock.GetPrintObjectAsync())
                .ReturnsAsync(new PrintObject { Record = new PrintRecord { Data = printData } });
        }

        private void SetupParty()
        {
            this._mockedQueryContext
                .Setup(mock => mock.GetPartyDataAsync(null, TestBsn, null, false))
                .ReturnsAsync(new CommonPartyData { Uri = s_partyUri });
        }

        private void SetupDocument()
        {
            this._mockedQueryContext
                .Setup(mock => mock.GetDocumentAsync(s_documentId))
                .ReturnsAsync(new SingularInformationObject { Content = s_contentUri.ToString() });

            this._mockedQueryContext
                .Setup(mock => mock.GetDocumentContentAsync(s_contentUri))
                .ReturnsAsync(Convert.ToBase64String(s_pdfBytes));
        }

        private static PrintNotifyReference GetReference()
            => new()
            {
                ObjectId = s_objectId,
                PartyId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Subject = TestSubject,
                AttachmentId = s_documentId,
            };

        private void SetupSuccessfulSend()
        {
            this._mockedNotifyClient
                .Setup(mock => mock.SendPrecompiledLetterAsync(It.IsAny<string>(), It.IsAny<byte[]>(), null))
                .ReturnsAsync(NotifySendResponse.Success());
        }
        #endregion
    }
}
