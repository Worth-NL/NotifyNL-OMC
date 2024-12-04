// © 2024, Worth Systems.

using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing;
using EventsHandler.Services.DataProcessing.Enums;
using EventsHandler.Services.DataProcessing.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Models.DTOs;
using EventsHandler.Services.DataProcessing.Strategy.Responses;
using EventsHandler.Services.Responding.Messages.Models.Details;
using EventsHandler.Services.Serialization.Interfaces;
using EventsHandler.Services.Validation.Interfaces;
using EventsHandler.Tests.Utilities._TestHelpers;
using Moq;
using System.Text.Json;

namespace EventsHandler.Tests.Unit.Services.DataProcessing
{
    [TestFixture]
    public sealed class NotifyProcessorTests
    {
        private Mock<ISerializationService> _mockedSerializer = null!;
        private Mock<IValidationService<NotificationEvent>> _mockedValidator = null!;
        private Mock<IScenariosResolver<INotifyScenario, NotificationEvent>> _mockedResolver = null!;

        private IProcessingService _processor = null!;

        [OneTimeSetUp]
        public void InitializeTests()
        {
            this._mockedSerializer = new Mock<ISerializationService>(MockBehavior.Strict);
            this._mockedValidator = new Mock<IValidationService<NotificationEvent>>(MockBehavior.Strict);
            this._mockedResolver = new Mock<IScenariosResolver<INotifyScenario, NotificationEvent>>(MockBehavior.Strict);
            
            this._processor = new NotifyProcessor(this._mockedSerializer.Object, this._mockedValidator.Object, this._mockedResolver.Object);
        }

        [SetUp]
        public void ResetTests()
        {
            // By default, Serializer would succeed
            this._mockedSerializer.Reset();
            this._mockedSerializer
                .Setup(mock => mock.Deserialize<NotificationEvent>(
                    It.IsAny<object>()))
                .Returns(NotificationEventHandler.GetNotification_Real_CaseCreateScenario_TheHague().Deserialized);
            
            // By default, Validator would succeed
            this._mockedValidator.Reset();
            this._mockedValidator
                .Setup(mock => mock.Validate(
                    ref It.Ref<NotificationEvent>.IsAny))
                .Returns(HealthCheck.OK_Valid);

            this._mockedResolver.Reset();
        }

        #region Test data
        private const string TestExceptionMessage = "Test exception message.";

        private static readonly string s_validNotification =
            NotificationEventHandler.GetNotification_Real_CaseUpdateScenario_TheHague();
        #endregion

        #region ProcessAsync()
        [Test]
        public async Task ProcessAsync_Failed_Deserialization_ReturnsProcessingResult_Skipped()
        {
            // Arrange
            this._mockedSerializer
                .Setup(mock => mock.Deserialize<NotificationEvent>(
                    It.IsAny<object>()))
                .Throws(() => new JsonException(TestExceptionMessage));  // NOTE: Overrule Serializer mock and make it fail

            // Act
            ProcessingResult result = await this._processor.ProcessAsync(new object());

            // Assert
            Assert.Multiple(() =>
            {
                VerifyMethodsCalls(1, 0, 0);

                Assert.That(result.Status, Is.EqualTo(ProcessingStatus.Skipped));
                Assert.That(result.Description, Is.EqualTo(ApiResources.Processing_STATUS_Notification
                    .Replace("{0}", TestExceptionMessage)
                    .Replace("{1}", "System.Object")));
            });
        }

        [Test]
        public async Task ProcessAsync_Failed_Validation_ReturnsProcessingResult_NotPossible()
        {
            // Arrange
            NotificationEvent notification = new()
            {
                Details = new ErrorDetails(TestExceptionMessage, string.Empty, [])
            };

            this._mockedSerializer
                .Setup(mock => mock.Deserialize<NotificationEvent>(
                    It.IsAny<object>()))
                .Returns(notification);

            this._mockedValidator
                .Setup(mock => mock.Validate(
                    ref notification))
                .Returns(HealthCheck.ERROR_Invalid);  // NOTE: Overrule Validator mock and make it fail

            // Act
            ProcessingResult result = await this._processor.ProcessAsync(new object());

            // Assert
            Assert.Multiple(() =>
            {
                VerifyMethodsCalls(1, 1, 0);

                Assert.That(result.Status, Is.EqualTo(ProcessingStatus.NotPossible));
                Assert.That(result.Description, Is.EqualTo(ApiResources.Processing_STATUS_Notification
                    .Replace("{0}", ApiResources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Message)
                    .Replace("{1}", "System.Object")));
            });
        }

        [Test]
        public async Task ProcessAsync_Failed_Ping_ReturnsProcessingResult_Skipped()
        {
            // Arrange
            NotificationEvent notificationJson = NotificationEventHandler.GetNotification_Test_Ping();

            this._mockedSerializer
                .Setup(mock => mock.Deserialize<NotificationEvent>(
                    It.IsAny<object>()))
                .Returns(notificationJson);

            // Act
            ProcessingResult result = await this._processor.ProcessAsync(notificationJson.Serialized());

            // Assert
            Assert.Multiple(() =>
            {
                VerifyMethodsCalls(1, 1, 0);

                Assert.That(result.Status, Is.EqualTo(ProcessingStatus.Skipped));
                Assert.That(result.Description, Is.EqualTo(ApiResources.Processing_STATUS_Notification
                    .Replace("{0}", ApiResources.Processing_ERROR_Notification_Test)
                    .Replace("{1}", "{\"actie\":\"-\",\"kanaal\":\"-\",\"resource\":\"-\",\"kenmerken\":{\"zaaktype\":null," +
                                    "\"bronorganisatie\":null,\"vertrouwelijkheidaanduiding\":null,\"objectType\":null,\"besluittype\":null," +
                                    "\"verantwoordelijkeOrganisatie\":null},\"hoofdObject\":\"http://some.hoofdobject.nl/\",\"resourceUrl\":" +
                                    "\"http://some.hoofdobject.nl/\",\"aanmaakdatum\":\"0001-01-01T00:00:00\"}")));
            });
        }

        [Test]
        public async Task ProcessAsync_Failed_UnknownScenario_ReturnsProcessingResult_Skipped()
        {
            // Arrange
            this._mockedResolver.Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .Throws<NotImplementedException>();

            // Act
            ProcessingResult result = await this._processor.ProcessAsync(s_validNotification);

            // Assert
            Assert.Multiple(() =>
            {
                VerifyMethodsCalls(1, 1, 1);

                Assert.That(result.Status, Is.EqualTo(ProcessingStatus.Skipped));
                Assert.That(result.Description, Is.EqualTo(ApiResources.Processing_STATUS_Notification
                    .Replace("{0}", ApiResources.Processing_ERROR_Scenario_NotImplemented)
                    .Replace("{1}", s_validNotification)));
            });
        }

        [Test]
        public async Task ProcessAsync_Failed_InternalErrors_WhenResolvingScenario_ReturnsProcessingResult_Failure()
        {
            // Arrange
            this._mockedResolver.Setup(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()))
                .Throws(() => new HttpRequestException(TestExceptionMessage));

            // Act
            ProcessingResult result = await this._processor.ProcessAsync(s_validNotification);

            // Assert
            Assert.Multiple(() =>
            {
                VerifyMethodsCalls(1, 1, 1);

                Assert.That(result.Status, Is.EqualTo(ProcessingStatus.Failure));
                Assert.That(result.Description, Is.EqualTo(ApiResources.Processing_STATUS_Notification
                    .Replace("{0}", nameof(HttpRequestException) + $" | {TestExceptionMessage}")
                    .Replace("{1}", s_validNotification)));
            });
        }

        [Test]
        public async Task ProcessAsync_Failed_TryGetData_ReturnsProcessingResult_Failure()
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
            Assert.Multiple(() =>
            {
                VerifyMethodsCalls(1, 1, 1);

                Assert.That(result.Status, Is.EqualTo(ProcessingStatus.Failure));
                Assert.That(result.Description, Is.EqualTo(ApiResources.Processing_STATUS_Notification
                    .Replace("{0}", ApiResources.Processing_ERROR_Scenario_NotificationNotSent
                        .Replace("{0}", ApiResources.Processing_ERROR_Scenario_NotificationMethod))
                    .Replace("{1}", s_validNotification)));
            });
        }

        [Test]
        public async Task ProcessAsync_Failed_ProcessData_ReturnsProcessingResult_Failure()
        {
            // Arrange
            var mockedNotifyScenario = new Mock<INotifyScenario>(MockBehavior.Strict);
            mockedNotifyScenario
                .Setup(mock => mock.TryGetDataAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(GettingDataResponse.Success(
                [
                    new NotifyData(NotifyMethods.Email)
                ]));

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
            Assert.Multiple(() =>
            {
                VerifyMethodsCalls(1, 1, 1);

                Assert.That(result.Status, Is.EqualTo(ProcessingStatus.Failure));
                Assert.That(result.Description, Is.EqualTo(ApiResources.Processing_STATUS_Notification
                    .Replace("{0}", ApiResources.Processing_ERROR_Scenario_NotificationNotSent
                        .Replace("{0}", processingErrorText))
                    .Replace("{1}", s_validNotification)));
            });
        }

        [Test]
        public async Task ProcessAsync_Success_ProcessData_ReturnsProcessingResult_Success()
        {
            // Arrange
            var mockedNotifyScenario = new Mock<INotifyScenario>(MockBehavior.Strict);
            mockedNotifyScenario
                .Setup(mock => mock.TryGetDataAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(GettingDataResponse.Success(
                [
                    new NotifyData(NotifyMethods.Email)
                ]));

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
            Assert.Multiple(() =>
            {
                VerifyMethodsCalls(1, 1, 1);

                Assert.That(result.Status, Is.EqualTo(ProcessingStatus.Success));
                Assert.That(result.Description, Is.EqualTo(ApiResources.Processing_STATUS_Notification
                    .Replace("{0}", ApiResources.Processing_SUCCESS_Scenario_NotificationSent)
                    .Replace("{1}", s_validNotification)));
            });
        }
        #endregion

        #region Verify
        private void VerifyMethodsCalls(int deserializeInvokeCount, int validateInvokeCount, int determineScenarioInvokeCount)
        {
            this._mockedSerializer
                .Verify(mock => mock.Deserialize<NotificationEvent>(
                        It.IsAny<object>()),
                    Times.Exactly(deserializeInvokeCount));

            this._mockedValidator
                .Verify(mock => mock.Validate(
                        ref It.Ref<NotificationEvent>.IsAny),
                    Times.Exactly(validateInvokeCount));

            this._mockedResolver
                .Verify(mock => mock.DetermineScenarioAsync(
                    It.IsAny<NotificationEvent>()),
                Times.Exactly(determineScenarioInvokeCount));
        }
        #endregion
    }
}
