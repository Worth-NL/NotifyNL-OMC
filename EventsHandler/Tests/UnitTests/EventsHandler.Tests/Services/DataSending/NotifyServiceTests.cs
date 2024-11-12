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
        public async Task SendSmsAsync_Using_GetDutchFallbackNumber_ForGivenMobileNumber_ConvertsMobileNumberToValid(string testMobileNumber, string expectedMobileNumber)
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
        private static NotifyService GetTestSendingService(
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