// © 2023, Worth Systems.

using EventsHandler.Services.Responding.Messages.Models.Base;
using EventsHandler.Services.Responding.Messages.Models.Details;
using EventsHandler.Services.Responding.Messages.Models.Errors;
using EventsHandler.Services.Responding.Messages.Models.Information;
using EventsHandler.Services.Responding.Messages.Models.Successes;
using EventsHandler.Services.Responding.Results.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventsHandler.UnitTests.Services.Responding.Results.Extensions
{
    [TestFixture]
    public sealed class ObjectResultExtensionsTests
    {
        private const string TestStatusDescription = "Test description";
        private const string TestMessage = "Test message";
        private const string TestCases = "Test case";
        private const string TestReason = "Test reason";

        [TestCaseSource(nameof(GetTestCases))]
        public void AsResult_ForResponses_Extensions_ReturnsExpectedObjectResult((Func<ObjectResult> Response, int StatusCode, string) test)
        {
            // Act
            ObjectResult actualResult = test.Response.Invoke();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.StatusCode, Is.EqualTo(test.StatusCode));

                if (actualResult.Value is BaseStandardResponseBody baseResponse)
                {
                    Assert.That(baseResponse.StatusCode, Is.EqualTo((HttpStatusCode)test.StatusCode));
                    Assert.That(baseResponse.StatusDescription, Is.Not.Empty);

                    if (actualResult.Value is BaseSimpleStandardResponseBody simpleResponse)
                    {
                        Assert.That(simpleResponse.Details.Message, Is.Not.Empty);

                        if (actualResult.Value is BaseEnhancedStandardResponseBody enhancedResponse)
                        {
                            Assert.That(enhancedResponse.Details.Cases, Is.Not.Empty);
                            Assert.That(enhancedResponse.Details.Reasons[0], Is.Not.Empty);
                        }
                    }
                }
                else
                {
                    Assert.Fail($"The response message has invalid type: {actualResult.Value!.GetType()}");
                }
            });
        }

        private static IEnumerable<(
            Func<ObjectResult> Response, int StatusCode, string Id)> GetTestCases()
        {
            // Arrange
            var testDetails = new InfoDetails(TestMessage, TestCases, [TestReason]);

            // Response-based extensions
            yield return (new ProcessingSucceeded(TestStatusDescription).AsResult_202, 202, "#1");
            yield return (new ProcessingSkipped(TestStatusDescription).AsResult_206, 206, "#2");
            yield return (new HttpRequestFailed.Simplified(testDetails).AsResult_400, 400, "#3");
            yield return (new HttpRequestFailed.Detailed(testDetails).AsResult_400, 400, "#4");
            yield return (new DeserializationFailed(testDetails).AsResult_422, 422, "#5");
            yield return (new InternalError(testDetails).AsResult_500, 500, "#6");
            yield return (new NotImplemented().AsResult_501, 501, "#7");

            yield return (new StandardResponseBody(HttpStatusCode.Accepted, TestMessage).AsResult_202, 202, "#8");
            yield return (new StandardResponseBody(HttpStatusCode.PartialContent, TestMessage).AsResult_206, 206, "#9");
            yield return (new StandardResponseBody(HttpStatusCode.BadRequest, TestMessage).AsResult_400, 400, "#10");
            yield return (new StandardResponseBody(HttpStatusCode.Forbidden, TestMessage).AsResult_403, 403, "#11");
            yield return (new StandardResponseBody(HttpStatusCode.InternalServerError, TestMessage).AsResult_500, 500, "#12");

            // Details-based extensions
            yield return (testDetails.AsResult_400, 400, "#13");
            yield return (testDetails.AsResult_422, 422, "#14");
            yield return (testDetails.AsResult_500, 500, "#15");

            // Simple static methods
            yield return (() => ObjectResultExtensions.AsResult_202(TestStatusDescription), 202, "#16");
            yield return (() => ObjectResultExtensions.AsResult_400(TestStatusDescription), 400, "#17");
            yield return (() => ObjectResultExtensions.AsResult_403(TestStatusDescription), 403, "#18");
            yield return (() => ObjectResultExtensions.AsResult_500(TestStatusDescription), 500, "#19");
            yield return (ObjectResultExtensions.AsResult_501, 501, "#20");
        }
    }
}