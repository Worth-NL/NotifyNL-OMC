// © 2024, Worth Systems.

using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Communication.Strategy.Interfaces;
using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Enums;
using EventsHandler.Behaviors.Mapping.Enums.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Exceptions;
using EventsHandler.Services.DataProcessing;
using EventsHandler.Services.DataProcessing.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Utilities._TestHelpers;
using Moq;
using ResourcesText = EventsHandler.Properties.Resources;

namespace EventsHandler.UnitTests.Services.DataProcessing
{
    [TestFixture]
    public sealed class DataProcessingTests
    {
        private IProcessingService<NotificationEvent>? _processor;
        private Mock<IScenariosResolver>? _mockedScenariosManager;
        private Mock<ISendingService<NotificationEvent, NotifyData>>? _mockedSender;

        [OneTimeSetUp]
        public void InitializeTests()
        {
            this._mockedScenariosManager = new Mock<IScenariosResolver>(MockBehavior.Strict);
            this._mockedSender = new Mock<ISendingService<NotificationEvent, NotifyData>>(MockBehavior.Strict);
            
            this._processor = new NotifyProcessor(_mockedScenariosManager.Object, _mockedSender.Object);
        }

        [SetUp]
        public void ResetTests()
        {
            this._mockedScenariosManager?.Reset();
            this._mockedSender?.Reset();
        }

        [Test]
        public async Task ProcessAsync_ForTestNotification_ReturnsProcessingResult_Skipped()
        {
            // Arrange
            var testUri = new Uri("http://some.hoofdobject.nl/");

            var testNotification = new NotificationEvent
            {
                Channel = Channels.Unknown,
                Resource = Resources.Unknown,
                MainObject = testUri,
                ResourceUrl = testUri
            };

            // Act
            (ProcessingResult status, string? message) = await this._processor!.ProcessAsync(testNotification);

            // Assert
            Verify_SendSmsAsync(Times.Never());
            Verify_SendEmailAsync(Times.Never());

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Skipped));
                Assert.That(message, Is.EqualTo(ResourcesText.Processing_ERROR_Notification_Test));
            });
        }

        [Test]
        public async Task ProcessAsync_ForMissingNotifyData_ReturnsProcessingResult_Failure()
        {
            // Arrange
            IMock<INotifyScenario> mockedNotifyScenario =
                GetMockedNotifyScenario(Array.Empty<NotifyData>());  // NOTE: Empty collection is invalid

            this._mockedScenariosManager?.Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(mockedNotifyScenario.Object);

            // Act
            (ProcessingResult status, string? message) = await this._processor!.ProcessAsync(
                NotificationEventHandler.GetNotification_Test_WithRegulars_ChannelAndSourceOrganization());

            // Assert
            Verify_SendSmsAsync(Times.Never());
            Verify_SendEmailAsync(Times.Never());

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Failure));
                Assert.That(message, Is.EqualTo(ResourcesText.Processing_ERROR_Scenario_DataNotFound));
            });
        }

        [TestCase(2, 0, 1)]  // Only: email
        [TestCase(3, 1, 0)]  // Only: SMS
        [TestCase(4, 1, 1)]  // Both: SMS + email
        public async Task ProcessAsync_ForValidNotifyData_WithValidNotifyMethods_CallsExpectedSendMethods_AndReturnsProcessingResult_Success(
            int notifyMethod, int smsSendCount, int emailSendCount)
        {
            // Arrange
            NotifyData testNotifyData = GetNotifyData((NotifyMethods)notifyMethod);
            SetupScenariosManager(testNotifyData);
            SetupSender(smsNotifyData:   smsSendCount   > 0 ? testNotifyData : default,
                        emailNotifyData: emailSendCount > 0 ? testNotifyData : default);

            // Act
            (ProcessingResult status, string? message) = await this._processor!.ProcessAsync(
                NotificationEventHandler.GetNotification_Test_WithRegulars_ChannelAndSourceOrganization());

            // Assert
            Verify_SendSmsAsync(smsSendCount     > 0 ? Times.Once() : Times.Never());
            Verify_SendEmailAsync(emailSendCount > 0 ? Times.Once() : Times.Never());

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Success));
                Assert.That(message, Is.EqualTo(ResourcesText.Processing_SUCCESS_Scenario_NotificationSent));
            });
        }

        [Test]
        public async Task ProcessAsync_ForValidNotifyData_WithInvalidNotifyMethod_DoesNotSendAnything_AndReturnsProcessingResult_Failure()
        {
            // Arrange
            NotifyData testNotifyData = GetNotifyData(NotifyMethods.None);
            SetupScenariosManager(testNotifyData);
            SetupSender();  // Nothing to be sent

            // Act
            (ProcessingResult status, string? message) = await this._processor!.ProcessAsync(
                NotificationEventHandler.GetNotification_Test_WithRegulars_ChannelAndSourceOrganization());

            // Assert
            Verify_SendSmsAsync(Times.Never());
            Verify_SendEmailAsync(Times.Never());

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Failure));
                Assert.That(message, Is.EqualTo(ResourcesText.Processing_ERROR_Scenario_NotificationNotSent));
            });
        }

        [Test]
        public async Task ProcessAsync_ForValidNotifyData_WhenTelemetryServiceIsDown_SendsNotification_AndReturnsProcessingResult_Success()
        {
            // Arrange
            NotifyData testNotifyData = GetNotifyData(NotifyMethods.Email);
            SetupScenariosManager(testNotifyData);

            this._mockedSender?.Setup(mock => mock.SendEmailAsync(
                    It.IsAny<NotificationEvent>(), It.IsAny<NotifyData>()))
                .Throws<TelemetryException>();

            // Act
            (ProcessingResult status, string? message) = await this._processor!.ProcessAsync(
                NotificationEventHandler.GetNotification_Test_WithRegulars_ChannelAndSourceOrganization());

            // Assert
            Verify_SendSmsAsync(Times.Never());
            Verify_SendEmailAsync(Times.Once());

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Success));
                Assert.That(message, Is.EqualTo(ResourcesText.Processing_ERROR_Telemetry_CompletionNotSent));
            });
        }

        [Test]
        public async Task ProcessAsync_ForScenarioThatCannotBeDetermined_ReturnsProcessingResult_Skipped()
        {
            // Arrange
            this._mockedScenariosManager?.Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .Throws<NotImplementedException>();

            // Act
            (ProcessingResult status, string? message) = await this._processor!.ProcessAsync(
                NotificationEventHandler.GetNotification_Test_WithRegulars_ChannelAndSourceOrganization());

            // Assert
            Verify_SendSmsAsync(Times.Never());
            Verify_SendEmailAsync(Times.Never());

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Skipped));
                Assert.That(message, Is.EqualTo(ResourcesText.Processing_ERROR_Scenario_NotImplemented));
            });
        }

        [Test]
        public async Task ProcessAsync_ForScenarioInternalErrors_ReturnsProcessingResult_Failure()
        {
            // Arrange
            this._mockedScenariosManager?.Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .Throws<HttpRequestException>();

            // Act
            (ProcessingResult status, string? message) = await this._processor!.ProcessAsync(
                NotificationEventHandler.GetNotification_Test_WithRegulars_ChannelAndSourceOrganization());

            // Assert
            Verify_SendSmsAsync(Times.Never());
            Verify_SendEmailAsync(Times.Never());

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Failure));
                Assert.That(message, Does.StartWith("Exception of type 'System.Net.Http.HttpRequestException' was"));
            });
        }

        #region Helper methods
        private static NotifyData GetNotifyData(NotifyMethods method)
        {
            return new NotifyData(method,
                string.Empty,
                string.Empty,
                new Dictionary<string, object>());
        }

        private static IMock<INotifyScenario> GetMockedNotifyScenario(NotifyData[] data)
        {
            var mockedNotifyScenario = new Mock<INotifyScenario>(MockBehavior.Strict);
            mockedNotifyScenario.Setup(mock => mock.GetAllNotifyDataAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(data);

            return mockedNotifyScenario;
        }

        private void SetupScenariosManager(NotifyData testNotifyData)
        {
            IMock<INotifyScenario> mockedNotifyScenario = GetMockedNotifyScenario(new[] { testNotifyData });
            
            this._mockedScenariosManager?.Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(mockedNotifyScenario.Object);
        }

        private void SetupSender(NotifyData? smsNotifyData = default, NotifyData? emailNotifyData = default)
        {
            this._mockedSender?.Setup(mock => mock.SendSmsAsync(
                    It.IsAny<NotificationEvent>(), smsNotifyData ?? It.IsAny<NotifyData>()))
                .Returns(Task.CompletedTask);

            this._mockedSender?.Setup(mock => mock.SendEmailAsync(
                    It.IsAny<NotificationEvent>(), emailNotifyData ?? It.IsAny<NotifyData>()))
                .Returns(Task.CompletedTask);
        }
        #endregion

        #region Verify methods
        private void Verify_SendSmsAsync(Times times)
        {
            this._mockedSender?.Verify(mock => mock.SendSmsAsync(
                It.IsAny<NotificationEvent>(), It.IsAny<NotifyData>()), times);
        }

        private void Verify_SendEmailAsync(Times times)
        {
            this._mockedSender?.Verify(mock => mock.SendEmailAsync(
                It.IsAny<NotificationEvent>(), It.IsAny<NotifyData>()), times);
        }
        #endregion
    }
}
