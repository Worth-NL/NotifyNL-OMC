// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataSending;
using EventsHandler.Services.DataSending.Clients.Factories.Interfaces;
using EventsHandler.Services.DataSending.Clients.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.DataSending.Responses;
using EventsHandler.Services.Serialization.Interfaces;
using EventsHandler.Utilities._TestHelpers;
using Moq;

namespace EventsHandler.UnitTests.Services.DataSending
{
    [TestFixture]
    public sealed class NotifyServiceTests
    {
        private INotifyService<NotifyData>? _testNotifyService;

        [TearDown]
        public void CleanupTests()
        {
            this._testNotifyService?.Dispose();
        }

        #region Caching NotificationClient
        [Test]
        public async Task HttpClient_IsCached_AsExpected()
        {
            #region First phase of the test (HttpClient will be just created)
            // Arrange
            Mock<INotifyClient> firstMockedNotifyClient = GetMockedNotifyClient();
            var firstMockedClientFactory = new Mock<IHttpClientFactory<INotifyClient, string>>(MockBehavior.Strict);
            firstMockedClientFactory
                .Setup(mock => mock.GetHttpClient(It.IsAny<string>()))
                .Returns(firstMockedNotifyClient.Object);

            Mock<ISerializationService> mockedSerializer = GetMockedSerializer();

            this._testNotifyService = new NotifyService(firstMockedClientFactory.Object, mockedSerializer.Object);

            NotificationEvent testNotification = NotificationEventHandler.GetNotification_Real_CaseUpdateScenario_TheHague().Deserialized();

            // Act
            await this._testNotifyService.SendEmailAsync(GetNotifyData(testNotification));

            // Assert
            firstMockedNotifyClient
                .Verify(mock => mock.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<string>()),
                Times.Once);
            #endregion

            #region Second phase of the test (HttpClient was already cached)
            // Arrange
            Mock<INotifyClient> secondMockedNotifyClient = GetMockedNotifyClient();
            var secondMockedClientFactory = new Mock<IHttpClientFactory<INotifyClient, string>>(MockBehavior.Strict);
            secondMockedClientFactory
                .Setup(mock => mock.GetHttpClient(It.IsAny<string>()))
                .Returns(secondMockedNotifyClient.Object);

            this._testNotifyService = new NotifyService(secondMockedClientFactory.Object, mockedSerializer.Object);

            // Act
            await this._testNotifyService.SendEmailAsync(new NotifyData());

            // Assert
            firstMockedNotifyClient
                .Verify(mock => mock.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<string>()),
                Times.Exactly(2));

            secondMockedNotifyClient
                .Verify(mock => mock.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<string>()),
                Times.Never);  // If the client is not cached, this method would be called again
            #endregion
        }
        #endregion

        #region SendEmailAsync()
        [Test]
        public async Task SendEmailAsync_Calls_NotificationClientMethod()
        {
            // Arrange
            Mock<INotifyClient> mockedClient = GetMockedNotifyClient();

            this._testNotifyService = GetTestSendingService(mockedClient);

            NotificationEvent testNotification = NotificationEventHandler.GetNotification_Real_CaseUpdateScenario_TheHague().Deserialized();

            // Act
            await this._testNotifyService.SendEmailAsync(GetNotifyData(testNotification));

            // Assert
            mockedClient
                .Verify(mock => mock.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<string>()),
                Times.Once);
        }
        #endregion

        #region SendSmsAsync()
        [Test]
        public async Task SendSmsAsync_Calls_NotificationClientMethod()
        {
            // Arrange
            Mock<INotifyClient> mockedClient = GetMockedNotifyClient();

            this._testNotifyService = GetTestSendingService(mockedClient);

            NotificationEvent testNotification = NotificationEventHandler.GetNotification_Real_CaseUpdateScenario_TheHague().Deserialized();

            // Act
            await this._testNotifyService.SendSmsAsync(GetNotifyData(testNotification));

            // Assert
            mockedClient
                .Verify(mock => mock.SendSmsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<string>()),
                Times.Once);
        }

        [TestCase("+31618758539", "+31618758539")]    // Nothing to be changed
        [TestCase("+442071234567", "+442071234567")]  // Nothing to be changed
        [TestCase("0618758539", "+31618758539")]      // The Dutch country code needs to be added
        public async Task GetDutchFallbackNumber_ForGivenMobileNumber_ConvertsMobileNumberToValid(string testMobileNumber, string expectedMobileNumber)
        {
            // Arrange
            Mock<INotifyClient> mockedClient = GetMockedNotifyClient();

            this._testNotifyService = GetTestSendingService(mockedClient);

            NotifyData testNotifyData = new
            (
                notificationMethod: NotifyMethods.Sms,
                contactDetails: testMobileNumber,
                templateId: default,
                personalization: default!,
                reference: new NotifyReference
                {
                    Notification = NotificationEventHandler.GetNotification_Real_CaseUpdateScenario_TheHague().Deserialized()
                }
            );

            // Act
            await this._testNotifyService.SendSmsAsync(testNotifyData);

            // Assert
            mockedClient
                .Verify(mock => mock.SendSmsAsync(
                    expectedMobileNumber,  // NOTE: The mock method will not be called if the phone number wouldn't be matching
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<string>()),
                Times.Once);
        }

        [Test, Ignore("Suspicious behavior on remote server")]
        public async Task GetEncoded_EncodesReferenceToBase64()
        {
            // Arrange
            Mock<INotifyClient> mockedClient = GetMockedNotifyClient();

            var mockedClientFactory = new Mock<IHttpClientFactory<INotifyClient, string>>();
            mockedClientFactory
                .Setup(mock => mock.GetHttpClient(It.IsAny<string>()))
                .Returns(mockedClient.Object);

            var notifyReference = new NotifyReference
            {
                Notification = NotificationEventHandler.GetNotification_Real_CaseUpdateScenario_TheHague().Deserialized(),
                CaseId = new Guid("12345678-1234-1234-1234-123456789012"),
                PartyId = new Guid("87654321-4321-4321-4321-210987654321")
            };

            const string serializedNotifyReference =
                "{\"Notification\":{\"actie\":\"create\",\"kanaal\":\"zaken\",\"resource\":\"status\",\"kenmerken\":" +
                "{\"zaaktype\":\"https://openzaak.test.denhaag.opengem.nl/catalogi/api/v1/zaaktypen/cf57c196-982d-4e2b-a567-d47794642bd7\"," +
                "\"bronorganisatie\":\"286130270\",\"vertrouwelijkheidaanduiding\":\"openbaar\",\"objectType\":\"http://0.0.0.0:0/\"," +
                "\"besluittype\":\"http://0.0.0.0:0/\",\"verantwoordelijkeOrganisatie\":\"\"},\"hoofdObject\":" +
                "\"https://openzaak.test.denhaag.opengem.nl/zaken/api/v1/zaken/4205aec5-9f5b-4abf-b177-c5a9946a77af\"," +
                "\"resourceUrl\":\"https://openzaak.test.denhaag.opengem.nl/zaken/api/v1/statussen/11cbdb9f-1445-4424-bf34-0bf066033e03\"," +
                "\"aanmaakdatum\":\"2023-09-22T11:41:46.0520000Z\"},\"CaseId\":\"12345678-1234-1234-1234-123456789012\"," +
                "\"PartyId\":\"87654321-4321-4321-4321-210987654321\"}";

            var mockedSerializer = new Mock<ISerializationService>();
            mockedSerializer
                .Setup(mock => mock.Serialize(notifyReference))
                .Returns(serializedNotifyReference);

            this._testNotifyService = new NotifyService(mockedClientFactory.Object, mockedSerializer.Object);

            NotifyData testNotifyData = new
            (
                notificationMethod: NotifyMethods.Sms,
                contactDetails: "112",
                templateId: default,
                personalization: default!,
                reference: notifyReference
            );

            // Act
            await this._testNotifyService.SendSmsAsync(testNotifyData);

            // Assert
            const string encodedReference =
                // ReSharper disable StringLiteralTypo
                "H4sIAAAAAAAACp1RwW7cIBD9F87BBozN2teceml6SC+9DTDeJeuFFcaJkij/3sFVlDZSD62xEH7zmPc875V9TSXMwUEJKbLplYErAd" +
                "nEXEYoyG7YGSLAQsgLnDESkHFNW3aVtBYo21pJGC+Ya51avACcy/O1Ek6lXNepbdMVY4WbgmtpPMYTwLGp6BEvTVxaMgBLOoYWrqF9" +
                "lO17j9i6uTdOjgMfD8pzjcpy6AfDvTZm1INW1htyYHOKKR8hhpX+pWqrwyA7oYyg6iPmktP2hEt4OJ8weIDot+BDPBKz+rAAmYjJPq" +
                "Ar9x/uybxo9jWJturgumyhlL8zSAtieUop+10O7/6wxd5u2Cml2d/tUv8ypD2BjwnVD61ED+h6Ps695RrszK00hrseRpoOGAPzb5l9" +
                "zzXJ/xP8FfZKgJTOejvOXGrdc62V5nbuNBd2FsMgug5FR6I04wt193TtUvMQquNi5ErdSzlpeodG9ErQ86MO5RZW/OKJKFWnKeEDr4" +
                "dPW8VHIRW1/wa5PO8XDmbodack/7QpKcb3Gnv7CVO18pvsAgAA";
                // ReSharper restore StringLiteralTypo

            mockedClient
                .Verify(mock => mock.SendSmsAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<Dictionary<string, object>>(),
                        encodedReference),
                    Times.Once);
        }
        #endregion

        #region GenerateTemplatePreviewAsync()
        [Test]
        public async Task GenerateTemplatePreviewAsync_Calls_NotificationClientMethod()
        {
            // Arrange
            Mock<INotifyClient> mockedClient = GetMockedNotifyClient();

            this._testNotifyService = GetTestSendingService(mockedClient);

            NotificationEvent testNotification =
                NotificationEventHandler.GetNotification_Real_CaseUpdateScenario_TheHague()
                    .Deserialized();

            // Act
            await this._testNotifyService.GenerateTemplatePreviewAsync(GetNotifyData(testNotification));

            // Assert
            mockedClient
                .Verify(mock => mock.GenerateTemplatePreviewAsync(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>()),
                Times.Once);
        }
        #endregion

        #region Setup
        private static INotifyService<NotifyData> GetTestSendingService(
            Mock<INotifyClient>? mockedClient = null)
        {
            var mockedClientFactory = new Mock<IHttpClientFactory<INotifyClient, string>>(MockBehavior.Strict);
            mockedClientFactory
                .Setup(mock => mock.GetHttpClient(
                    It.IsAny<string>()))
                .Returns((mockedClient ?? GetMockedNotifyClient()).Object);

            Mock<ISerializationService> mockedSerializer = GetMockedSerializer();

            return new NotifyService(mockedClientFactory.Object, mockedSerializer.Object);
        }

        private static Mock<INotifyClient> GetMockedNotifyClient()
        {
            Mock<INotifyClient> notificationClientMock = new(MockBehavior.Strict);
            notificationClientMock
                .Setup(mock => mock.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Success());

            notificationClientMock
                .Setup(mock => mock.SendSmsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<string>()))
                .ReturnsAsync(NotifySendResponse.Success());

            notificationClientMock
                .Setup(mock => mock.GenerateTemplatePreviewAsync(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(NotifyTemplateResponse.Success("Test subject", "Test body"));

            return notificationClientMock;
        }

        private static Mock<ISerializationService> GetMockedSerializer()
        {
            Mock<ISerializationService> serializerMock = new(MockBehavior.Strict);
            serializerMock
                .Setup(mock => mock.Serialize(
                    It.IsAny<NotifyReference>()))
                .Returns(string.Empty);

            serializerMock
                .Setup(mock => mock.Deserialize<NotifyReference>(
                    It.IsAny<string>()))
                .Returns(new NotifyReference());

            return serializerMock;
        }
        #endregion

        #region Helper methods
        private static NotifyData GetNotifyData(NotificationEvent notification)
        {
            return new NotifyData
            (
                notificationMethod: default,
                contactDetails: string.Empty,
                templateId: default,
                personalization: null!,
                reference: new NotifyReference
                {
                    Notification = notification,
                    CaseId = Guid.Empty,
                    PartyId = Guid.Empty
                }
            );
        }
        #endregion
    }
}