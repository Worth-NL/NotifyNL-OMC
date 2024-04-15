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

                if (actualResult.Value is BaseApiStandardResponseBody baseResponse)
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
            var testDetails = new InfoDetails(TestMessage, TestCases, new[] { TestReason });
            var testEnhancedResponse = new HttpRequestFailed(testDetails);
            var testSimpleResponse = new ProcessingSkipped(TestStatusDescription);

            // Response-based extensions
            yield return (testEnhancedResponse.AsResult_202, 202, "#1");
            yield return (testSimpleResponse.AsResult_206,   206, "#2");
            yield return (testSimpleResponse.AsResult_400,   400, "#3");
            yield return (testEnhancedResponse.AsResult_422, 422, "#4");

            // Details-based extensions
            yield return (testDetails.AsResult_400, 400, "#5");
            yield return (testDetails.AsResult_422, 422, "#6");
            yield return (testDetails.AsResult_500, 500, "#7");

            // Parameterless extensions
            yield return (ObjectResultExtensions.AsResult_501, 501, "#8");
        }
    }
}