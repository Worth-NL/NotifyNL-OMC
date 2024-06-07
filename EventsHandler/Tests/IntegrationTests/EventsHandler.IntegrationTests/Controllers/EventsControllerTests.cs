// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Responding.Messages.Models.Base;
using EventsHandler.Behaviors.Responding.Messages.Models.Details;
using EventsHandler.Behaviors.Responding.Messages.Models.Errors;
using EventsHandler.Behaviors.Responding.Messages.Models.Successes;
using EventsHandler.Behaviors.Responding.Results.Builder;
using EventsHandler.Behaviors.Versioning;
using EventsHandler.Controllers;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Interfaces;
using EventsHandler.Services.Serialization.Interfaces;
using EventsHandler.Services.UserCommunication;
using EventsHandler.Services.UserCommunication.Interfaces;
using EventsHandler.Services.Validation.Interfaces;
using EventsHandler.Utilities._TestHelpers;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace EventsHandler.IntegrationTests.Controllers
{
    [TestFixture]
    public sealed class EventsControllerTests
    {
        private static readonly object s_testJson = new();

        private Mock<ISerializationService> _serializerMock = new();
        private Mock<IValidationService<NotificationEvent>> _validatorMock = new();
        private Mock<IProcessingService<NotificationEvent>> _processorMock = new();
        private Mock<IRespondingService<NotificationEvent>> _responderMock = new();
        private Mock<IVersionsRegister> _registerMock = new();

        [OneTimeSetUp]
        public void InitializeMocks()
        {
            this._serializerMock = new Mock<ISerializationService>(MockBehavior.Strict);
            this._validatorMock = new Mock<IValidationService<NotificationEvent>>(MockBehavior.Strict);
            this._processorMock = new Mock<IProcessingService<NotificationEvent>>(MockBehavior.Strict);
            this._responderMock = new Mock<IRespondingService<NotificationEvent>>(MockBehavior.Strict);
            this._registerMock = new Mock<IVersionsRegister>(MockBehavior.Strict);
        }

        [SetUp]
        public void InitializeTests()
        {
            this._serializerMock.Reset();
            this._serializerMock.Setup(mock => mock.Deserialize<NotificationEvent>(
                    It.IsAny<object>()))
                .Returns(NotificationEventHandler.GetNotification_Test_WithOrphans_ManuallyCreated);

            this._validatorMock.Reset();
            this._validatorMock.Setup(mock => mock.Validate(
                    ref It.Ref<NotificationEvent>.IsAny))
                .Returns(HealthCheck.OK_Inconsistent);
            
            this._processorMock.Reset();
            this._responderMock.Reset();
        }

        #region Testing IActionResult API responses
        [Test]
        public async Task ListenAsync_UnexpectedFailure_ReturnsExpectedIActionResult()
        {
            // Arrange
            this._serializerMock.Setup(mock => mock.Deserialize<NotificationEvent>(It.IsAny<object>()))
                                .Throws<JsonException>();

            EventsController testController = GetTestEventsController_WithRealResponder();

            // Act
            IActionResult actualResult = await testController.ListenAsync(s_testJson);

            // Assert
            AssertWithConditions<UnprocessableEntityObjectResult, DeserializationFailed>(
                actualResult,
                HttpStatusCode.UnprocessableEntity,
                Resources.Operation_RESULT_Deserialization_Failure,
                Resources.Deserialization_ERROR_InvalidJson_Message);
        }

        [Test]
        public async Task ListenAsync_HttpFailure_ReturnsExpectedIActionResult()
        {
            // Arrange
            this._processorMock.Setup(mock => mock.ProcessAsync(It.IsAny<NotificationEvent>()))
                               .Throws<HttpRequestException>();

            EventsController testController = GetTestEventsController_WithRealResponder();

            // Act
            IActionResult actualResult = await testController.ListenAsync(s_testJson);

            // Assert
            AssertWithConditions<BadRequestObjectResult, HttpRequestFailed>(
                actualResult,
                HttpStatusCode.BadRequest,
                Resources.Operation_RESULT_HttpRequest_Failure,
                Resources.HttpRequest_ERROR_Message);
        }

        [Test]
        public async Task ListenAsync_HealthCheck_ERROR_Invalid_ReturnsExpectedIActionResult()
        {
            // Arrange
            this._validatorMock.Setup(mock => mock.Validate(ref It.Ref<NotificationEvent>.IsAny))
                               .Returns((ref NotificationEvent notificationEvent) =>
                               {
                                   notificationEvent.Details = GetTestErrorDetails();

                                   return HealthCheck.ERROR_Invalid;
                               });

            EventsController testController = GetTestEventsController_WithRealResponder();

            // Act
            IActionResult actualResult = await testController.ListenAsync(s_testJson);

            // Assert
            AssertWithConditions<UnprocessableEntityObjectResult, ProcessingFailed.Detailed>(
                actualResult,
                HttpStatusCode.UnprocessableEntity,
                Resources.Processing_ERROR_Scenario_NotificationNotSent + AddNotificationDetails(s_testJson),
                Resources.Operation_RESULT_Deserialization_Failure);
        }

        [Test]
        public async Task ListenAsync_HealthCheck_OK_Inconsistent_ReturnsExpectedIActionResult()
        {
            // Arrange
            this._validatorMock.Setup(mock => mock.Validate(ref It.Ref<NotificationEvent>.IsAny))
                               .Returns((ref NotificationEvent notificationEvent) =>
                               {
                                   notificationEvent.Details = GetTestInfoDetails_Partial();

                                   return HealthCheck.OK_Inconsistent;
                               });

            this._processorMock.Setup(mock => mock.ProcessAsync(It.IsAny<NotificationEvent>()))
                               .ReturnsAsync((ProcessingResult.Success, Resources.Processing_SUCCESS_Scenario_NotificationSent));

            EventsController testController = GetTestEventsController_WithRealResponder();

            // Act
            IActionResult actualResult = await testController.ListenAsync(s_testJson);

            // Assert
            AssertWithConditions<ObjectResult, ProcessingSucceeded>(
                actualResult,
                HttpStatusCode.Accepted,
                Resources.Processing_SUCCESS_Scenario_NotificationSent + AddNotificationDetails(s_testJson),
                Resources.Operation_RESULT_Deserialization_Partial);
        }

        [Test]
        public async Task ListenAsync_HealthCheck_OK_Valid_ReturnsExpectedIActionResult()
        {
            // Arrange
            this._validatorMock.Setup(mock => mock.Validate(ref It.Ref<NotificationEvent>.IsAny))
                               .Returns((ref NotificationEvent notificationEvent) =>
                               {
                                   notificationEvent.Details = GetTestInfoDetails_Success();

                                   return HealthCheck.OK_Valid;
                               });

            this._processorMock.Setup(mock => mock.ProcessAsync(It.IsAny<NotificationEvent>()))
                               .ReturnsAsync((ProcessingResult.Success, Resources.Processing_SUCCESS_Scenario_NotificationSent));

            EventsController testController = GetTestEventsController_WithRealResponder();

            // Act
            IActionResult actualResult = await testController.ListenAsync(s_testJson);

            // Assert
            AssertWithConditions<ObjectResult, ProcessingSucceeded>(
                actualResult,
                HttpStatusCode.Accepted,
                Resources.Processing_SUCCESS_Scenario_NotificationSent + AddNotificationDetails(s_testJson),
                Resources.Operation_RESULT_Deserialization_Success);
        }
        #endregion

        #region Helper methods
        private static ErrorDetails GetTestErrorDetails()
        {
            return new ErrorDetails(Resources.Operation_RESULT_Deserialization_Failure, string.Empty, Array.Empty<string>());
        }

        private static InfoDetails GetTestInfoDetails_Partial()
        {
            return new InfoDetails(Resources.Operation_RESULT_Deserialization_Partial, string.Empty, Array.Empty<string>());
        }

        private static InfoDetails GetTestInfoDetails_Success()
        {
            return new InfoDetails(Resources.Operation_RESULT_Deserialization_Success, string.Empty, Array.Empty<string>());
        }

        private EventsController GetTestEventsController_WithRealResponder()
        {
            return new EventsController(ConfigurationHandler.GetValidAppSettingsConfiguration(),
                                        this._serializerMock.Object, this._validatorMock.Object,
                                        this._processorMock.Object, GetRealResponderService(),
                                        this._registerMock.Object);
        }

        private static IRespondingService<NotificationEvent> GetRealResponderService()
        {
            return new NotificationResponder(new DetailsBuilder());
        }

        private static void AssertWithConditions
            <TExpectedApiActionResultType, TExpectedApiResponseBodyType>
            (
                IActionResult actualResult,
                HttpStatusCode expectedHttpStatusCode,
                string expectedStatusDescription,
                string expectedDetailsMessage
            )
            where TExpectedApiActionResultType : IActionResult
            where TExpectedApiResponseBodyType : BaseEnhancedStandardResponseBody
        {
            Assert.Multiple(() =>
            {
                Assert.That(actualResult, Is.TypeOf<TExpectedApiActionResultType>());

                var castResult = (ObjectResult)actualResult;
                Assert.That(castResult.StatusCode, Is.EqualTo((int)expectedHttpStatusCode));

                if (castResult.Value is TExpectedApiResponseBodyType apiResponse)
                {
                    Assert.That(apiResponse.StatusCode, Is.EqualTo(expectedHttpStatusCode));
                    Assert.That(apiResponse.StatusDescription, Is.EqualTo(expectedStatusDescription));
                    Assert.That(apiResponse.Details.Message, Is.EqualTo(expectedDetailsMessage));
                }
                else
                {
                    Assert.Fail($"Wrong type of API response body.{Environment.NewLine}" +
                        $"Expected: {typeof(TExpectedApiResponseBodyType)}{Environment.NewLine}" +
                        $"But was: {castResult.Value!.GetType()}");
                }
            });
        }

        private static string AddNotificationDetails(object json)
        {
            return $" | Notification: {json}";
        }
        #endregion
    }
}