// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataSending;
using EventsHandler.Services.DataSending.Clients.Factories.Interfaces;
using EventsHandler.Services.DataSending.Clients.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.DataSending.Responses;
using EventsHandler.Services.Serialization.Interfaces;
using EventsHandler.Utilities._TestHelpers;

namespace EventsHandler.IntegrationTests.Services.DataSending
{
    [TestFixture]
    public sealed class NotifyServiceTests
    {
        private INotifyService<NotificationEvent, NotifyData>? _testNotifyService;

        [TearDown]
        public void CleanupTests()
        {
            this._testNotifyService?.Dispose();
        }

        #region SendEmailAsync
        [Test]
        public async Task SendEmailAsync_Calls_NotificationClientMethod()
        {
            // Arrange
            Mock<INotifyClient> mockedClient = GetMockedNotifyClient();

            this._testNotifyService = GetTestSendingService(mockedClient);

            NotificationEvent testNotification =
                NotificationEventHandler.GetNotification_Real_CasesScenario_TheHague()
                    .Deserialized();

            // Act
            await this._testNotifyService.SendEmailAsync(testNotification, new NotifyData());

            // Assert
            mockedClient.Verify(mock =>
                mock.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<string>()), Times.Once);
        }
        #endregion

        #region SendSmsAsync
        [Test]
        public async Task SendSmsAsync_Calls_NotificationClientMethod()
        {
            // Arrange
            Mock<INotifyClient> mockedClient = GetMockedNotifyClient();

            this._testNotifyService = GetTestSendingService(mockedClient);

            NotificationEvent testNotification =
                NotificationEventHandler.GetNotification_Real_CasesScenario_TheHague()
                    .Deserialized();

            // Act
            await this._testNotifyService.SendSmsAsync(testNotification, new NotifyData());

            // Assert
            mockedClient.Verify(mock =>
                mock.SendSmsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<string>()), Times.Once);
        }

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

            NotificationEvent testNotification =
                NotificationEventHandler.GetNotification_Real_CasesScenario_TheHague()
                    .Deserialized();

            // Act
            await this._testNotifyService.SendEmailAsync(testNotification, new NotifyData());

            // Assert
            firstMockedNotifyClient.Verify(mock => mock.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()), Times.Once);
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
            await this._testNotifyService.SendEmailAsync(testNotification, new NotifyData());

            // Assert
            firstMockedNotifyClient.Verify(mock => mock.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()), Times.Exactly(2));

            secondMockedNotifyClient.Verify(mock => mock.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()), Times.Never);  // If the client is not cached, this method would be called again
            #endregion
        }
        #endregion

        #region Helper methods
        private static INotifyService<NotificationEvent, NotifyData> GetTestSendingService(
            Mock<INotifyClient>? mockedClient = null)
        {
            var mockedClientFactory = new Mock<IHttpClientFactory<INotifyClient, string>>(MockBehavior.Strict);
            mockedClientFactory.Setup(mock => mock.GetHttpClient(It.IsAny<string>()))
                               .Returns((mockedClient ?? GetMockedNotifyClient()).Object);

            Mock<ISerializationService> mockedSerializer = GetMockedSerializer();

            return new NotifyService(mockedClientFactory.Object, mockedSerializer.Object);
        }

        private static Mock<INotifyClient> GetMockedNotifyClient()
        {
            var notificationClientMock = new Mock<INotifyClient>(MockBehavior.Strict);

            notificationClientMock.Setup(mock => mock.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new NotifySendResponse(true, "Test Email body"));

            notificationClientMock.Setup(mock => mock.SendSmsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new NotifySendResponse(true, "Test SMS body"));

            return notificationClientMock;
        }

        private static Mock<ISerializationService> GetMockedSerializer()
        {
            var serializerMock = new Mock<ISerializationService>(MockBehavior.Strict);

            serializerMock.Setup(mock => mock.Serialize(
                    It.IsAny<NotificationEvent>()))
                .Returns(string.Empty);

            serializerMock.Setup(mock => mock.Deserialize<NotificationEvent>(
                    It.IsAny<string>()))
                .Returns(new NotificationEvent());

            return serializerMock;
        }
        #endregion
    }
}