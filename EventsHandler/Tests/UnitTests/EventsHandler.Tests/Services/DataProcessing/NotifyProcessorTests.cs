// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataProcessing;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.DataProcessing.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataProcessing.Strategy.Responses;
using EventsHandler.Services.Serialization.Interfaces;
using EventsHandler.Services.Validation.Interfaces;
using EventsHandler.Utilities._TestHelpers;
using Moq;
using System.Text.Json;
using ResourcesText = EventsHandler.Properties.Resources;

namespace EventsHandler.UnitTests.Services.DataProcessing
{
    [TestFixture]
    public sealed class NotifyProcessorTests
    {
        private Mock<ISerializationService> _serializerMock = null!;
        private Mock<IValidationService<NotificationEvent>> _validatorMock = null!;
        private Mock<IScenariosResolver<INotifyScenario, NotificationEvent>> _mockedResolver = null!;

        private IProcessingService _processor = null!;

        [OneTimeSetUp]
        public void InitializeTests()
        {
            this._serializerMock = new Mock<ISerializationService>(MockBehavior.Strict);
            this._validatorMock = new Mock<IValidationService<NotificationEvent>>(MockBehavior.Strict);
            this._mockedResolver = new Mock<IScenariosResolver<INotifyScenario, NotificationEvent>>(MockBehavior.Strict);
            
            this._processor = new NotifyProcessor(this._serializerMock.Object, this._validatorMock.Object, this._mockedResolver.Object);
        }

        [SetUp]
        public void ResetTests()
        {
            this._serializerMock.Reset();
            this._serializerMock
                .Setup(mock => mock.Deserialize<NotificationEvent>(
                    It.IsAny<object>()))
                .Returns(NotificationEventHandler.GetNotification_Real_CaseCreateScenario_TheHague().Deserialized);
            
            this._validatorMock.Reset();
            this._validatorMock
                .Setup(mock => mock.Validate(
                    ref It.Ref<NotificationEvent>.IsAny))
                .Returns(HealthCheck.OK_Valid);

            this._mockedResolver.Reset();
        }

        #region Test data
        private static readonly string s_validNotification =
            NotificationEventHandler.GetNotification_Real_CaseUpdateScenario_TheHague();
        #endregion

        #region ProcessAsync()
        [Test]
        public async Task ProcessAsync_Failed_Deserialization_ReturnsProcessingResult_Skipped()
        {
            // Arrange
            this._serializerMock
                .Setup(mock => mock.Deserialize<NotificationEvent>(
                    It.IsAny<object>()))
                .Throws<JsonException>();

            // Act
            ProcessingResult result = await this._processor.ProcessAsync(new object());

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(ProcessingStatus.Skipped));
                Assert.That(result.Description, Is.EqualTo("Exception of type 'System.Text.Json.JsonException' was thrown. | Notification: System.Object."));
            });
        }

        [Test]
        public async Task ProcessAsync_TestNotification_ReturnsProcessingResult_Skipped()
        {
            // Act
            ProcessingResult result = await this._processor.ProcessAsync(NotificationEventHandler.GetNotification_Test_Ping());

            // Assert
            VerifyMethodsCalls(0);

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(ProcessingStatus.Skipped));
                Assert.That(result.Description, Is.EqualTo(ResourcesText.Processing_ERROR_Notification_Test));
            });
        }

        [Test]
        public async Task ProcessAsync_ValidNotification_UnknownScenario_ReturnsProcessingResult_Skipped()
        {
            // Arrange
            this._mockedResolver.Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .Throws<NotImplementedException>();

            // Act
            ProcessingResult result = await this._processor.ProcessAsync(s_validNotification);

            // Assert
            VerifyMethodsCalls(1);

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(ProcessingStatus.Skipped));
                Assert.That(result.Description, Is.EqualTo(ResourcesText.Processing_ERROR_Scenario_NotImplemented));
            });
        }

        [Test]
        public async Task ProcessAsync_InternalErrors_WhenResolvingScenario_ReturnsProcessingResult_Failure()
        {
            // Arrange
            this._mockedResolver.Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .Throws<HttpRequestException>();

            // Act
            ProcessingResult result = await this._processor.ProcessAsync(s_validNotification);

            // Assert
            VerifyMethodsCalls(1);

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(ProcessingStatus.Failure));
                Assert.That(result.Description, Does.StartWith(
                    $"{nameof(HttpRequestException)} | Exception of type '{typeof(HttpRequestException).FullName}' was"));
            });
        }

        [Test]
        public async Task ProcessAsync_ValidNotification_ValidScenario_FailedGetDataResponse_ReturnsProcessingResult_Failure()
        {
            // Arrange
            var mockedNotifyScenario = new Mock<INotifyScenario>(MockBehavior.Strict);
            mockedNotifyScenario
                .Setup(mock => mock.TryGetDataAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(GettingDataResponse.Failure());
            
            this._mockedResolver
                .Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(mockedNotifyScenario.Object);
            
            // Act
            ProcessingResult result = await this._processor.ProcessAsync(s_validNotification);

            // Assert
            VerifyMethodsCalls(1);

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(ProcessingStatus.Failure));
                Assert.That(result.Description, Is.EqualTo(
                    ResourcesText.Processing_ERROR_Scenario_NotificationNotSent.Replace("{0}", ResourcesText.Processing_ERROR_Scenario_NotificationMethod)));
            });
        }

        [Test]
        public async Task ProcessAsync_ValidNotification_ValidScenario_SuccessGetDataResponse_FailedProcessDataResponse_ReturnsProcessingResult_Failure()
        {
            // Arrange
            var mockedNotifyScenario = new Mock<INotifyScenario>(MockBehavior.Strict);
            mockedNotifyScenario
                .Setup(mock => mock.TryGetDataAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(GettingDataResponse.Success(new[]
                {
                    new NotifyData(NotifyMethods.Email)
                }));

            const string processingErrorText = "HTTP Bad Request";
            mockedNotifyScenario
                .Setup(mock => mock.ProcessDataAsync(
                    It.IsAny<NotificationEvent>(),
                    It.IsAny<IReadOnlyCollection<NotifyData>>()))
                .ReturnsAsync(ProcessingDataResponse.Failure(processingErrorText));
            
            this._mockedResolver
                .Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(mockedNotifyScenario.Object);
            
            // Act
            ProcessingResult result = await this._processor.ProcessAsync(s_validNotification);

            // Assert
            VerifyMethodsCalls(1);

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(ProcessingStatus.Failure));
                Assert.That(result.Description, Is.EqualTo(
                    ResourcesText.Processing_ERROR_Scenario_NotificationNotSent.Replace("{0}", processingErrorText)));
            });
        }

        [Test]
        public async Task ProcessAsync_ValidNotification_ValidScenario_SuccessGetDataResponse_SuccessProcessDataResponse_ReturnsProcessingResult_Success()
        {
            // Arrange
            var mockedNotifyScenario = new Mock<INotifyScenario>(MockBehavior.Strict);
            mockedNotifyScenario
                .Setup(mock => mock.TryGetDataAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(GettingDataResponse.Success(new[]
                {
                    new NotifyData(NotifyMethods.Email)
                }));

            mockedNotifyScenario
                .Setup(mock => mock.ProcessDataAsync(
                    It.IsAny<NotificationEvent>(),
                    It.IsAny<IReadOnlyCollection<NotifyData>>()))
                .ReturnsAsync(ProcessingDataResponse.Success);
            
            this._mockedResolver
                .Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(mockedNotifyScenario.Object);
            
            // Act
            ProcessingResult result = await this._processor.ProcessAsync(s_validNotification);

            // Assert
            VerifyMethodsCalls(1);

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(ProcessingStatus.Success));
                Assert.That(result.Description, Is.EqualTo(ResourcesText.Processing_SUCCESS_Scenario_NotificationSent));
            });
        }
        #endregion

        #region Verify
        private void VerifyMethodsCalls(int determineScenarioInvokeCount)
        {
            this._mockedResolver
                .Verify(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()),
                Times.Exactly(determineScenarioInvokeCount));
        }
        #endregion
    }
}
