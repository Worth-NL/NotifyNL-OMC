// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Responding.Messages.Models.Base;
using EventsHandler.Behaviors.Responding.Messages.Models.Details;
using EventsHandler.Behaviors.Responding.Messages.Models.Errors;
using EventsHandler.Behaviors.Responding.Messages.Models.Successes;
using EventsHandler.Behaviors.Responding.Results.Builder;
using EventsHandler.Controllers;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Interfaces;
using EventsHandler.Services.Responding;
using EventsHandler.Services.Responding.Interfaces;
using EventsHandler.Services.Serialization.Interfaces;
using EventsHandler.Services.Validation.Interfaces;
using EventsHandler.Services.Versioning.Interfaces;
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

        private Mock<ISerializationService> _serializerMock = null!;
        private Mock<IValidationService<NotificationEvent>> _validatorMock = null!;
        private Mock<IProcessingService<NotificationEvent>> _processorMock = null!;
        private Mock<IRespondingService<NotificationEvent>> _responderMock = null!;
        private Mock<IVersionsRegister> _registerMock = null!;

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
                .Returns(NotificationEventHandler.GetNotification_Test_EmptyAttributes_WithOrphans_ManuallyCreated);

            this._validatorMock.Reset();
            this._validatorMock.Setup(mock => mock.Validate(
                    ref It.Ref<NotificationEvent>.IsAny))
                .Returns(HealthCheck.OK_Inconsistent);

            this._processorMock.Reset();
            this._responderMock.Reset();
        }

        #region Testing IActionResult API responses
        [Test]
        public async Task ListenAsync_Failure_Deserialize_ReturnsErrorResult()
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
                Resources.Operation_ERROR_Deserialization_Failure,
                Resources.Deserialization_ERROR_InvalidJson_Message);
        }

        [Test]
        public async Task ListenAsync_Failure_Validate_ReturnsErrorResult()
        {
            // Arrange
            this._validatorMock.Setup(mock => mock.Validate(ref It.Ref<NotificationEvent>.IsAny))
                               .Returns((ref NotificationEvent notificationEvent) =>
                               {
                                   notificationEvent.Details = GetTestErrorDetails_Notification_Properties();  // NOTE: Other ErrorDetails are also possible, but that's covered in Validator tests

                                   return HealthCheck.ERROR_Invalid;
                               });

            EventsController testController = GetTestEventsController_WithRealResponder();

            // Act
            IActionResult actualResult = await testController.ListenAsync(s_testJson);

            // Assert
            AssertWithConditions<UnprocessableEntityObjectResult, DeserializationFailed>(
                actualResult,
                HttpStatusCode.UnprocessableEntity,
                Resources.Operation_ERROR_Deserialization_Failure,
                Resources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Message);
        }

        [Test]
        public async Task ListenAsync_Failure_ProcessAsync_ReturnsErrorResult()
        {
            // Arrange
            this._processorMock.Setup(mock => mock.ProcessAsync(It.IsAny<NotificationEvent>()))
                               .Throws<HttpRequestException>();

            EventsController testController = GetTestEventsController_WithRealResponder();

            // Act
            IActionResult actualResult = await testController.ListenAsync(s_testJson);

            // Assert
            AssertWithConditions<BadRequestObjectResult, HttpRequestFailed.Detailed>(
                actualResult,
                HttpStatusCode.BadRequest,
                Resources.Operation_ERROR_HttpRequest_Failure,
                Resources.HttpRequest_ERROR_Message);
        }

        [Test]
        public async Task ListenAsync_Success_Validate_HealthCheck_OK_Inconsistent_ReturnsInfoResult()
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
                Resources.Operation_SUCCESS_Deserialization_Partial);
        }

        [Test]
        public async Task ListenAsync_Success_Validate_HealthCheck_OK_Valid_ReturnsInfoResult()
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
                Resources.Operation_SUCCESS_Deserialization_Success);
        }
        #endregion

        #region Helper methods
        private static ErrorDetails GetTestErrorDetails_Notification_Properties()
        {
            return new ErrorDetails(
                Resources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Message,
                "hoofdObject, resourceUrl",
                new[]
                {
                    Resources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Reason1,
                    Resources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Reason2,
                    Resources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Reason3
                });
        }

        private static InfoDetails GetTestInfoDetails_Partial()
        {
            return new InfoDetails(Resources.Operation_SUCCESS_Deserialization_Partial, string.Empty, Array.Empty<string>());
        }

        private static InfoDetails GetTestInfoDetails_Success()
        {
            return new InfoDetails(Resources.Operation_SUCCESS_Deserialization_Success, string.Empty, Array.Empty<string>());
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
            return new OmcResponder(new DetailsBuilder());
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
        {
            Assert.Multiple(() =>
            {
                Assert.That(actualResult, Is.TypeOf<TExpectedApiActionResultType>());

                var castResult = (ObjectResult)actualResult;
                Assert.That(castResult.StatusCode, Is.EqualTo((int)expectedHttpStatusCode), "Status code");

                if (castResult.Value is BaseStandardResponseBody simpleResponse)
                {
                    Assert.That(simpleResponse.StatusCode, Is.EqualTo(expectedHttpStatusCode), "Status code");
                    Assert.That(simpleResponse.StatusDescription, Is.EqualTo(expectedStatusDescription), "Status description");

                    if (castResult.Value is BaseEnhancedStandardResponseBody enhancedResponse)
                    {
                        Assert.That(enhancedResponse.Details.Message, Is.EqualTo(expectedDetailsMessage), "Details message");
                    }
                }
                else
                {
                    Assert.Fail($"Wrong type of API response body.{Environment.NewLine}" +
                        $"Expected: {typeof(TExpectedApiResponseBodyType)}{Environment.NewLine}" +
                        $"But was: {castResult.Value!.GetType()}");
                }
            });
        }

        private static string AddNotificationDetails(object json) => $" | Notification: {json}";
        #endregion
    }
}