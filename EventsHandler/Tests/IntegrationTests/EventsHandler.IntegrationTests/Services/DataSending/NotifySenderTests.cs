// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataReceiving.Factories.Interfaces;
using EventsHandler.Services.DataSending;
using EventsHandler.Services.DataSending.Clients.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.Telemetry.Interfaces;

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
                .SendSmsAsync("+0000000000", "00000000-0000-0000-0000-000000000000", new Dictionary<string, object>());

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task SendEmailAsync_ReturnsTrue_AsExpected()
        {
            // Act
            bool result = await GetMockedNotifyClient().Object
                .SendEmailAsync("123@gmail.com", "00000000-0000-0000-0000-000000000000", new Dictionary<string, object>());

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
            NotificationEvent testNotification = GetTestNotification();

            // Act
            await this._testNotifySender.SendSmsAsync(testNotification, new NotifyData());

            // Assert
            mockedClient.Verify(mock =>
                mock.SendSmsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Test]
        public async Task SendEmailAsync_Calls_NotificationClientMethod_SendEmailAsync()
        {
            // Arrange
            Mock<INotifyClient> mockedClient = GetMockedNotifyClient();
            this._testNotifySender = GetTestSendingService(mockedClient: mockedClient);
            NotificationEvent testNotification = GetTestNotification();

            // Act
            await this._testNotifySender.SendEmailAsync(testNotification, new NotifyData());

            // Assert
            mockedClient.Verify(mock =>
                mock.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>()), Times.Once);
        }
        
        [Test]
        public async Task SendSmsAsync_Calls_IFeedbackServiceMethod_ReportCompletionAsync()
        {
            // Arrange
            Mock<IFeedbackService> mockedTelemetry = GetMockedTelemetry();
            this._testNotifySender = GetTestSendingService(mockedTelemetry: mockedTelemetry);
            NotificationEvent testNotification = GetTestNotification();

            // Act
            await this._testNotifySender.SendSmsAsync(testNotification, new NotifyData());

            // Assert
            mockedTelemetry.Verify(mock =>
                mock.ReportCompletionAsync(
                    It.IsAny<NotificationEvent>(),
                    It.IsAny<NotifyMethods>()), Times.Once);
        }

        [Test]
        public async Task SendEmailAsync_Calls_IFeedbackServiceMethod_ReportCompletionAsync()
        {
            // Arrange
            Mock<IFeedbackService> mockedTelemetry = GetMockedTelemetry();
            this._testNotifySender = GetTestSendingService(mockedTelemetry: mockedTelemetry);
            NotificationEvent testNotification = GetTestNotification();

            // Act
            await this._testNotifySender.SendEmailAsync(testNotification, new NotifyData());

            // Assert
            mockedTelemetry.Verify(mock => mock.ReportCompletionAsync(
                It.IsAny<NotificationEvent>(),
                It.IsAny<NotifyMethods>()), Times.Once);
        }

        [Test]
        public async Task HttpClient_IsCached_AsExpected()
        {
            #region First phase of the test (HttpClient will be just created)
            // Arrange
            Mock<IFeedbackService> mockedTelemetry = GetMockedTelemetry();
            Mock<INotifyClient> firstMockedNotifyClient = GetMockedNotifyClient();
            var firstMockedClientFactory = new Mock<IHttpClientFactory<INotifyClient, string>>(MockBehavior.Strict);
            firstMockedClientFactory
                .Setup(mock => mock.GetHttpClient(It.IsAny<string>()))
                .Returns(firstMockedNotifyClient.Object);

            this._testNotifySender = new NotifySender(firstMockedClientFactory.Object, mockedTelemetry.Object);

            NotificationEvent testNotification = GetTestNotification();
            
            // Act
            await this._testNotifySender.SendEmailAsync(testNotification, new NotifyData());

            // Assert
            firstMockedNotifyClient.Verify(mock => mock.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()), Times.Once);
            #endregion

            #region Second phase of the test (HttpClient was already cached)
            // Arrange
            Mock<INotifyClient> secondMockedNotifyClient = GetMockedNotifyClient();
            var secondMockedClientFactory = new Mock<IHttpClientFactory<INotifyClient, string>>(MockBehavior.Strict);
            secondMockedClientFactory
                .Setup(mock => mock.GetHttpClient(It.IsAny<string>()))
                .Returns(secondMockedNotifyClient.Object);

            this._testNotifySender = new NotifySender(secondMockedClientFactory.Object, mockedTelemetry.Object);
            
            // Act
            await this._testNotifySender.SendEmailAsync(testNotification, new NotifyData());

            // Assert
            firstMockedNotifyClient.Verify(mock => mock.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()), Times.Exactly(2));

            secondMockedNotifyClient.Verify(mock => mock.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()), Times.Never);
            #endregion
        }
        #endregion

        #region Helper methods
        private static ISendingService<NotificationEvent, NotifyData> GetTestSendingService(
            Mock<INotifyClient>? mockedClient = null,
            Mock<IFeedbackService>? mockedTelemetry = null)
        {
            var mockedClientFactory = new Mock<IHttpClientFactory<INotifyClient, string>>(MockBehavior.Strict);
            mockedClientFactory.Setup(mock => mock.GetHttpClient(It.IsAny<string>()))
                               .Returns((mockedClient ?? GetMockedNotifyClient()).Object);

            mockedTelemetry ??= GetMockedTelemetry();

            return new NotifySender(mockedClientFactory.Object, mockedTelemetry.Object);
        }

        private static Mock<INotifyClient> GetMockedNotifyClient()
        {
            var notificationClientMock = new Mock<INotifyClient>(MockBehavior.Strict);

            notificationClientMock.Setup(mock => mock.SendSmsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(true);

            notificationClientMock.Setup(mock => mock.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(true);

            return notificationClientMock;
        }

        private static Mock<IFeedbackService> GetMockedTelemetry()
        {
            var mockedTelemetry = new Mock<IFeedbackService>(MockBehavior.Strict);
            mockedTelemetry.Setup(mock => mock.ReportCompletionAsync(
                    It.IsAny<NotificationEvent>(),
                    It.IsAny<NotifyMethods>()))
                .ReturnsAsync(string.Empty);

            return mockedTelemetry;
        }

        private static NotificationEvent GetTestNotification() => new()
        {
            Attributes = new EventAttributes { SourceOrganization = new string('0', 9) }
        };
        #endregion
    }
}