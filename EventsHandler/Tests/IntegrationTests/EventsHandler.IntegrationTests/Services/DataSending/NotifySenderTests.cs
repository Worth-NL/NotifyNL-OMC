// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataReceiving.Factories.Interfaces;
using EventsHandler.Services.DataSending;
using EventsHandler.Services.DataSending.Clients.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.Serialization.Interfaces;
using EventsHandler.Utilities._TestHelpers;

#pragma warning disable IDE0008 // Use explicit type

namespace EventsHandler.IntegrationTests.Services.DataSending
{
    [TestFixture]
    public sealed class NotifySenderTests
    {
        private ISendingService<NotificationEvent, NotifyData>? _testNotifySender;

        [TearDown]
        public void CleanupTests()
        {
            this._testNotifySender?.Dispose();
        }

        #region INotifyClient tests
        [Test]
        public async Task SendSmsAsync_ReturnsTrue_AsExpected()
        {
            // Act
            bool result = await GetMockedNotifyClient().Object
                .SendSmsAsync("+0000000000", "00000000-0000-0000-0000-000000000000", new Dictionary<string, object>(), string.Empty);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task SendEmailAsync_ReturnsTrue_AsExpected()
        {
            // Act
            bool result = await GetMockedNotifyClient().Object
                .SendEmailAsync("123@gmail.com", "00000000-0000-0000-0000-000000000000", new Dictionary<string, object>(), string.Empty);

            // Assert
            Assert.That(result, Is.True);
        }
        #endregion

        #region ISendingService tests
        [Test]
        public async Task SendSmsAsync_Calls_NotificationClientMethod_SendSmsAsync()
        {
            // Arrange
            Mock<INotifyClient> mockedClient = GetMockedNotifyClient();

            this._testNotifySender = GetTestSendingService(mockedClient: mockedClient);

            NotificationEvent testNotification =
                NotificationEventHandler.GetNotification_Real_CasesScenario_TheHague()
                    .Deserialized();

            // Act
            await this._testNotifySender.SendSmsAsync(testNotification, new NotifyData());

            // Assert
            mockedClient.Verify(mock =>
                mock.SendSmsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task SendEmailAsync_Calls_NotificationClientMethod_SendEmailAsync()
        {
            // Arrange
            Mock<INotifyClient> mockedClient = GetMockedNotifyClient();

            this._testNotifySender = GetTestSendingService(mockedClient: mockedClient);

            NotificationEvent testNotification =
                NotificationEventHandler.GetNotification_Real_CasesScenario_TheHague()
                    .Deserialized();

            // Act
            await this._testNotifySender.SendEmailAsync(testNotification, new NotifyData());

            // Assert
            mockedClient.Verify(mock =>
                mock.SendEmailAsync(
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

            this._testNotifySender = new NotifySender(firstMockedClientFactory.Object, mockedSerializer.Object);

            NotificationEvent testNotification =
                NotificationEventHandler.GetNotification_Real_CasesScenario_TheHague()
                    .Deserialized();

            // Act
            await this._testNotifySender.SendEmailAsync(testNotification, new NotifyData());

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

            this._testNotifySender = new NotifySender(secondMockedClientFactory.Object, mockedSerializer.Object);

            // Act
            await this._testNotifySender.SendEmailAsync(testNotification, new NotifyData());

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
                It.IsAny<string>()), Times.Never);
            #endregion
        }
        #endregion

        #region Helper methods
        private static ISendingService<NotificationEvent, NotifyData> GetTestSendingService(
            Mock<INotifyClient>? mockedClient = null)
        {
            var mockedClientFactory = new Mock<IHttpClientFactory<INotifyClient, string>>(MockBehavior.Strict);
            mockedClientFactory.Setup(mock => mock.GetHttpClient(It.IsAny<string>()))
                               .Returns((mockedClient ?? GetMockedNotifyClient()).Object);

            Mock<ISerializationService> mockedSerializer = GetMockedSerializer();

            return new NotifySender(mockedClientFactory.Object, mockedSerializer.Object);
        }

        private static Mock<INotifyClient> GetMockedNotifyClient()
        {
            var notificationClientMock = new Mock<INotifyClient>(MockBehavior.Strict);

            notificationClientMock.Setup(mock => mock.SendSmsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<string>()))
                .ReturnsAsync(true);

            notificationClientMock.Setup(mock => mock.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<string>()))
                .ReturnsAsync(true);

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