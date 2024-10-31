// © 2023, Worth Systems.

using EventsHandler.Controllers;
using EventsHandler.Mapping.Enums;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Interfaces;
using EventsHandler.Services.DataProcessing.Strategy.Responses;
using EventsHandler.Services.Responding;
using EventsHandler.Services.Responding.Interfaces;
using EventsHandler.Services.Responding.Messages.Models.Base;
using EventsHandler.Services.Responding.Messages.Models.Details;
using EventsHandler.Services.Responding.Messages.Models.Errors;
using EventsHandler.Services.Responding.Messages.Models.Successes;
using EventsHandler.Services.Responding.Results.Builder;
using EventsHandler.Services.Versioning.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventsHandler.IntegrationTests.Controllers
{
    [TestFixture]
    public sealed class EventsControllerTests
    {
        private Mock<IProcessingService> _processorMock = null!;
        private Mock<IRespondingService<NotificationEvent>> _responderMock = null!;
        private Mock<IVersionsRegister> _registerMock = null!;

        [OneTimeSetUp]
        public void InitializeMocks()
        {
            this._processorMock = new Mock<IProcessingService>(MockBehavior.Strict);
            this._responderMock = new Mock<IRespondingService<NotificationEvent>>(MockBehavior.Strict);
            this._registerMock = new Mock<IVersionsRegister>(MockBehavior.Strict);
        }

        [SetUp]
        public void InitializeTests()
        {
            this._processorMock.Reset();
            this._responderMock.Reset();
            this._registerMock.Reset();
        }

        #region Testing IActionResult API responses
        [Test]
        public async Task ListenAsync_Failure_ProcessAsync_ReturnsErrorResult()
        {
            // Arrange
            this._processorMock.Setup(mock => mock.ProcessAsync(It.IsAny<NotificationEvent>()))
                               .Throws<HttpRequestException>();

            EventsController testController = GetTestEventsController_WithRealResponder();

            // Act
            IActionResult actualResult = await testController.ListenAsync(default!);

            // Assert
            AssertWithConditions<BadRequestObjectResult, HttpRequestFailed.Detailed>(
                actualResult,
                HttpStatusCode.BadRequest,
                Resources.Operation_ERROR_HttpRequest_Failure,
                Resources.HttpRequest_ERROR_Message);
        }

        [Test]
        public async Task ListenAsync_Success_Validate_HealthCheck_OK_Valid_ReturnsInfoResult()
        {
            // Arrange
            this._processorMock
                .Setup(mock => mock.ProcessAsync(
                    It.IsAny<NotificationEvent>()))
                .ReturnsAsync(
                    new ProcessingResult(ProcessingStatus.Success, Resources.Processing_SUCCESS_Scenario_NotificationSent, default!, GetTestInfoDetails_Success()));

            EventsController testController = GetTestEventsController_WithRealResponder();

            // Act
            IActionResult actualResult = await testController.ListenAsync(default!);

            // Assert
            AssertWithConditions<ObjectResult, ProcessingSucceeded>(
                actualResult,
                HttpStatusCode.Accepted,
                Resources.Processing_SUCCESS_Scenario_NotificationSent,
                Resources.Operation_SUCCESS_Deserialization_Success);
        }
        #endregion

        #region Helper methods
        private static InfoDetails GetTestInfoDetails_Partial()
            => new(Resources.Operation_SUCCESS_Deserialization_Partial, string.Empty, Array.Empty<string>());

        private static InfoDetails GetTestInfoDetails_Success()
            => new(Resources.Operation_SUCCESS_Deserialization_Success, string.Empty, Array.Empty<string>());

        private EventsController GetTestEventsController_WithRealResponder()
        {
            return new EventsController(this._processorMock.Object, GetRealResponderService(),
                                        this._registerMock.Object);
        }

        private static IRespondingService<ProcessingResult> GetRealResponderService() => new OmcResponder(new DetailsBuilder());

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
        #endregion
    }
}