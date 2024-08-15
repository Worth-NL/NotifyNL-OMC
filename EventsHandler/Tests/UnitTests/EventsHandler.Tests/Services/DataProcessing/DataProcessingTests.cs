// © 2024, Worth Systems.

using EventsHandler.Exceptions;
using EventsHandler.Mapping.Enums;
using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataProcessing;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.DataProcessing.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.DataSending.Responses;
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
        private Mock<INotifyService<NotificationEvent, NotifyData>>? _mockedSender;

        [OneTimeSetUp]
        public void InitializeTests()
        {
            this._mockedScenariosManager = new Mock<IScenariosResolver>(MockBehavior.Strict);
            this._mockedSender = new Mock<INotifyService<NotificationEvent, NotifyData>>(MockBehavior.Strict);

            this._processor = new NotifyProcessor(this._mockedScenariosManager.Object, this._mockedSender.Object);
        }

        [SetUp]
        public void ResetTests()
        {
            this._mockedScenariosManager?.Reset();
            this._mockedSender?.Reset();
        }

        #region ProcessAsync()
        [Test]
        public async Task ProcessAsync_ForTestNotification_ReturnsProcessingResult_Skipped()
        {
            // Arrange
            var testUri = new Uri("http://some.hoofdobject.nl/");

            var testNotification = new NotificationEvent
            {
                Channel = Channels.Unknown,
                Resource = Resources.Unknown,
                MainObjectUri = testUri,
                ResourceUri = testUri
            };

            // Act
            (ProcessingResult status, string? message) = await this._processor!.ProcessAsync(testNotification);

            // Assert
            Verify_SendEmailAsync(Times.Never());
            Verify_SendSmsAsync(Times.Never());

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
                NotificationEventHandler.GetNotification_Test_EmptyAttributes_With_Channel_And_SourceOrganization_ManuallyCreated());

            // Assert
            Verify_SendEmailAsync(Times.Never());
            Verify_SendSmsAsync(Times.Never());

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Failure));
                Assert.That(message, Is.EqualTo(ResourcesText.Processing_ERROR_Scenario_DataNotFound));
            });
        }

        [TestCase(2, 1, 0)]  // Only: Email
        [TestCase(3, 0, 1)]  // Only: SMS
        public async Task ProcessAsync_ForValidNotifyData_WithValidNotifyMethods_CallsExpectedSendMethods_AndReturnsProcessingResult_Success(
                int notifyMethod, int emailSendCount, int smsSendCount)
        {
            // Arrange
            NotifyData testNotifyData = GetNotifyData((NotifyMethods)notifyMethod);
            SetupScenariosManager(testNotifyData);
            SetupSender(emailSendCount > 0 ? testNotifyData : default,
                        smsSendCount > 0 ? testNotifyData : default);
            // Act
            (ProcessingResult status, string? message) = await this._processor!.ProcessAsync(
                NotificationEventHandler.GetNotification_Test_EmptyAttributes_With_Channel_And_SourceOrganization_ManuallyCreated());

            // Assert
            Verify_SendEmailAsync(emailSendCount > 0 ? Times.Once() : Times.Never());
            Verify_SendSmsAsync(smsSendCount     > 0 ? Times.Once() : Times.Never());

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
                NotificationEventHandler.GetNotification_Test_EmptyAttributes_With_Channel_And_SourceOrganization_ManuallyCreated());

            // Assert
            Verify_SendEmailAsync(Times.Never());
            Verify_SendSmsAsync(Times.Never());

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

            const string exceptionMessage = "Test";

            this._mockedSender?.Setup(mock => mock.SendEmailAsync(
                    It.IsAny<NotificationEvent>(), It.IsAny<NotifyData>()))
                .Throws(new TelemetryException(exceptionMessage));

            // Act
            (ProcessingResult status, string? message) = await this._processor!.ProcessAsync(
                NotificationEventHandler.GetNotification_Test_EmptyAttributes_With_Channel_And_SourceOrganization_ManuallyCreated());

            // Assert
            Verify_SendEmailAsync(Times.Once());
            Verify_SendSmsAsync(Times.Never());

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Success));
                Assert.That(message, Is.EqualTo($"{ResourcesText.Processing_ERROR_Telemetry_CompletionNotSent} | {exceptionMessage}"));
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
                NotificationEventHandler.GetNotification_Test_EmptyAttributes_With_Channel_And_SourceOrganization_ManuallyCreated());

            // Assert
            Verify_SendEmailAsync(Times.Never());
            Verify_SendSmsAsync(Times.Never());

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
                NotificationEventHandler.GetNotification_Test_EmptyAttributes_With_Channel_And_SourceOrganization_ManuallyCreated());

            // Assert
            Verify_SendEmailAsync(Times.Never());
            Verify_SendSmsAsync(Times.Never());

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Failure));
                Assert.That(message, Does.StartWith($"{nameof(HttpRequestException)} | Exception of type '{typeof(HttpRequestException).FullName}' was"));
            });
        }
        #endregion

        #region Helper methods
        private static NotifyData GetNotifyData(NotifyMethods method)
        {
            return new NotifyData(method,
                string.Empty,
                Guid.Empty,
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

        private void SetupSender(NotifyData? emailNotifyData = default, NotifyData? smsNotifyData = default)
        {
            this._mockedSender?.Setup(mock => mock.SendEmailAsync(
                    It.IsAny<NotificationEvent>(), emailNotifyData ?? It.IsAny<NotifyData>()))
                .ReturnsAsync(new NotifySendResponse(true, "Test Email body"));

            this._mockedSender?.Setup(mock => mock.SendSmsAsync(
                    It.IsAny<NotificationEvent>(), smsNotifyData ?? It.IsAny<NotifyData>()))
                .ReturnsAsync(new NotifySendResponse(true, "Test SMS body"));
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
