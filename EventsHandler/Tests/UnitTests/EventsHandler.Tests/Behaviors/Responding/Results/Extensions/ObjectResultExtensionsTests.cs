// © 2023, Worth Systems.

using EventsHandler.Behaviors.Responding.Messages.Models.Base;
using EventsHandler.Behaviors.Responding.Messages.Models.Details;
using EventsHandler.Behaviors.Responding.Messages.Models.Errors;
using EventsHandler.Behaviors.Responding.Messages.Models.Information;
using EventsHandler.Behaviors.Responding.Results.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace EventsHandler.UnitTests.Behaviors.Responding.Results.Extensions
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
            var testSimpleResponse = new ProcessingSkipped(TestStatusDescription);

            var testDetails = new InfoDetails(TestMessage, TestCases, new[] { TestReason });
            var testDetailedResponse = new DeserializationFailed(testDetails);

            // Response-based extensions
            yield return (testSimpleResponse.AsResult_206,   206, "#1");
            yield return (testSimpleResponse.AsResult_400,   400, "#2");
            
            yield return (testDetailedResponse.AsResult_202, 202, "#3");
            yield return (testDetailedResponse.AsResult_206, 206, "#4");
            yield return (testDetailedResponse.AsResult_400, 400, "#5");
            yield return (testDetailedResponse.AsResult_422, 422, "#6");

            // Details-based extensions
            yield return (testDetails.AsResult_400, 400, "#7");
            yield return (testDetails.AsResult_422, 422, "#8");
            yield return (testDetails.AsResult_500, 500, "#9");

            // Simple static methods
            yield return (() => ObjectResultExtensions.AsResult_202(TestStatusDescription), 202, "#10");
            yield return (() => ObjectResultExtensions.AsResult_400(TestStatusDescription), 400, "#11");
            yield return (() => ObjectResultExtensions.AsResult_403(TestStatusDescription), 403, "#12");
            yield return (() => ObjectResultExtensions.AsResult_500(TestStatusDescription), 500, "#13");
            yield return (ObjectResultExtensions.AsResult_501, 501, "#14");
        }
    }
}