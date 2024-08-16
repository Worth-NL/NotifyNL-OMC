// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums;
using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataProcessing;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.DataProcessing.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataProcessing.Strategy.Responses;
using EventsHandler.Utilities._TestHelpers;
using Moq;
using ResourcesText = EventsHandler.Properties.Resources;

namespace EventsHandler.UnitTests.Services.DataProcessing
{
    [TestFixture]
    public sealed class DataProcessingTests
    {
        private Mock<IScenariosResolver> _mockedScenariosResolver = null!;
        private IProcessingService<NotificationEvent> _processor = null!;

        private static readonly NotificationEvent s_validNotification =
            NotificationEventHandler.GetNotification_Real_CasesScenario_TheHague().Deserialized();

        [OneTimeSetUp]
        public void InitializeTests()
        {
            this._mockedScenariosResolver = new Mock<IScenariosResolver>(MockBehavior.Strict);
            this._processor = new NotifyProcessor(this._mockedScenariosResolver.Object);
        }

        [SetUp]
        public void ResetTests()
        {
            this._mockedScenariosResolver.Reset();
        }

        #region ProcessAsync()
        [Test]
        public async Task ProcessAsync_TestNotification_ReturnsProcessingResult_Skipped()
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
            (ProcessingResult status, string? message) = await this._processor.ProcessAsync(testNotification);

            // Assert
            VerifyMethodsCalls(0);

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Skipped));
                Assert.That(message, Is.EqualTo(ResourcesText.Processing_ERROR_Notification_Test));
            });
        }

        [Test]
        public async Task ProcessAsync_ValidNotification_UnknownScenario_ReturnsProcessingResult_Skipped()
        {
            // Arrange
            this._mockedScenariosResolver.Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .Throws<NotImplementedException>();

            // Act
            (ProcessingResult status, string? message) = await this._processor.ProcessAsync(s_validNotification);

            // Assert
            VerifyMethodsCalls(1);

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Skipped));
                Assert.That(message, Is.EqualTo(ResourcesText.Processing_ERROR_Scenario_NotImplemented));
            });
        }

        [Test]
        public async Task ProcessAsync_InternalErrors_WhenResolvingScenario_ReturnsProcessingResult_Failure()
        {
            // Arrange
            this._mockedScenariosResolver.Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .Throws<HttpRequestException>();

            // Act
            (ProcessingResult status, string? message) = await this._processor.ProcessAsync(s_validNotification);

            // Assert
            VerifyMethodsCalls(1);

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Failure));
                Assert.That(message, Does.StartWith($"{nameof(HttpRequestException)} | Exception of type '{typeof(HttpRequestException).FullName}' was"));
            });
        }

        [Test]
        public async Task ProcessAsync_ValidNotification_MissingNotifyData_ReturnsProcessingResult_Failure()
        {
            // Arrange
            var mockedNotifyScenario = new Mock<INotifyScenario>(MockBehavior.Strict);
            mockedNotifyScenario
                .Setup(mock => mock.GetAllNotifyDataAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(Array.Empty<NotifyData>());  // NOTE: Empty collection is invalid

            this._mockedScenariosResolver.Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(mockedNotifyScenario.Object);

            // Act
            (ProcessingResult status, string? message) = await this._processor.ProcessAsync(s_validNotification);

            // Assert
            VerifyMethodsCalls(1);

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Failure));
                Assert.That(message, Is.EqualTo(ResourcesText.Processing_ERROR_Scenario_DataNotFound));
            });
        }

        [Test]
        public async Task ProcessAsync_ValidNotification_ValidNotifyData_InvalidNotifyMethod_ReturnsProcessingResult_Failure()
        {
            // Arrange
            var mockedNotifyScenario = new Mock<INotifyScenario>(MockBehavior.Strict);
            mockedNotifyScenario
                .Setup(mock => mock.GetAllNotifyDataAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(new[]
                {
                    GetNotifyData(NotifyMethods.None)
                });

            mockedNotifyScenario
                .Setup(mock => mock.ProcessDataAsync(
                    It.IsAny<NotificationEvent>(),
                    It.IsAny<IEnumerable<NotifyData>>()))
                .ReturnsAsync(ProcessingResponse.Failure());
            
            this._mockedScenariosResolver
                .Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(mockedNotifyScenario.Object);
            
            // Act
            (ProcessingResult status, string? message) = await this._processor.ProcessAsync(s_validNotification);

            // Assert
            VerifyMethodsCalls(1);

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Failure));
                Assert.That(message, Is.EqualTo(ResourcesText.Processing_ERROR_Scenario_NotificationNotSent));
            });
        }

        [Test]
        public async Task ProcessAsync_ValidNotification_ValidNotifyData_ValidNotifyMethod_ReturnsProcessingResult_Success()
        {
            // Arrange
            var mockedNotifyScenario = new Mock<INotifyScenario>(MockBehavior.Strict);
            mockedNotifyScenario
                .Setup(mock => mock.GetAllNotifyDataAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(new[]
                {
                    GetNotifyData(NotifyMethods.Email),
                    GetNotifyData(NotifyMethods.Sms)
                });

            mockedNotifyScenario
                .Setup(mock => mock.ProcessDataAsync(
                    It.IsAny<NotificationEvent>(),
                    It.IsAny<IEnumerable<NotifyData>>()))
                .ReturnsAsync(ProcessingResponse.Success);
            
            this._mockedScenariosResolver
                .Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(mockedNotifyScenario.Object);
            
            // Act
            (ProcessingResult status, string? message) = await this._processor.ProcessAsync(s_validNotification);

            // Assert
            VerifyMethodsCalls(1);

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(ProcessingResult.Success));
                Assert.That(message, Is.EqualTo(ResourcesText.Processing_SUCCESS_Scenario_NotificationSent));
            });
        }
        #endregion

        #region Setup
        private static NotifyData GetNotifyData(NotifyMethods method)
        {
            return new NotifyData(method,
                string.Empty,
                Guid.Empty,
                new Dictionary<string, object>());
        }
        #endregion

        #region Verify
        private void VerifyMethodsCalls(int determineScenarioInvokeCount)
        {
            this._mockedScenariosResolver
                .Verify(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()),
                Times.Exactly(determineScenarioInvokeCount));
        }
        #endregion
    }
}
