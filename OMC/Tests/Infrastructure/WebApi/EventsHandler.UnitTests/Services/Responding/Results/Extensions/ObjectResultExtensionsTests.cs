// © 2023, Worth Systems.

using Common.Models.Messages.Base;
using Common.Models.Messages.Details;
using Common.Models.Messages.Errors;
using Common.Models.Messages.Errors.Specific;
using Common.Models.Messages.Information;
using Common.Models.Messages.Successes;
using Common.Models.Responses;
using Common.Properties;
using EventsHandler.Services.Responding.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventsHandler.Tests.Unit.Services.Responding.Results.Extensions
{
    [TestFixture]
    public sealed class ObjectResultExtensionsTests
    {
        #region Test data
        private const string TestDescription = "Test description";
        private const string TestJson = "{ }";
        private const string TestMessage = "Test message";
        private const string TestCases = "Case 1, Case 2";
        private static readonly string[] s_testReasons = ["Reason 1, Reason2, Reason 3"];

        private static readonly InfoDetails s_infoDetails = new(TestMessage, TestCases, s_testReasons);
        private static readonly ErrorDetails s_errorDetails = new(TestMessage, TestCases, s_testReasons);

        private static readonly ProcessingResult s_successResult = ProcessingResult.Success(TestDescription, TestJson, s_infoDetails);
        private static readonly ProcessingResult s_failedResult = ProcessingResult.Failure(TestDescription, TestJson, s_errorDetails);
        #endregion

        [TestCaseSource(nameof(GetTestCases))]
        public void AsResult_ForResponses_Extensions_ReturnsExpectedObjectResult((string Id, Func<ObjectResult> Response, int StatusCode, string Description) test)
        {
            // Act
            ObjectResult actualResult = test.Response.Invoke();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResult.StatusCode, Is.EqualTo(test.StatusCode), FailedTestMessage(test.Id));

                if (actualResult.Value is BaseStandardResponseBody baseResponse)
                {
                    Assert.That(baseResponse.StatusCode, Is.EqualTo((HttpStatusCode)test.StatusCode), FailedTestMessage(test.Id));
                    Assert.That(baseResponse.StatusDescription, Is.EqualTo(test.Description), FailedTestMessage(test.Id));

                    if (actualResult.Value is BaseSimpleStandardResponseBody simpleResponse)
                    {
                        Assert.That(simpleResponse.Details.Message, Is.EqualTo(TestMessage), FailedTestMessage(test.Id));

                        if (actualResult.Value is BaseEnhancedStandardResponseBody enhancedResponse)
                        {
                            Assert.That(enhancedResponse.Details.Cases, Is.EqualTo(TestCases), FailedTestMessage(test.Id));
                            Assert.That(enhancedResponse.Details.Reasons, Has.Length.EqualTo(s_testReasons.Length), FailedTestMessage(test.Id));
                        }
                    }
                }
                else
                {
                    Assert.Fail($"{FailedTestMessage(test.Id)} | The response message has invalid type: {actualResult.Value!.GetType()}");
                }
            });

            return;

            static string FailedTestMessage(string testId)
            {
                return $"Test {testId} failed.";
            }
        }

        private static IEnumerable<(
            string Id, Func<ObjectResult> Response, int StatusCode, string Description)> GetTestCases()
        {
            // Response-based extensions
            yield return ("#01", new ProcessingSucceeded(s_successResult).AsResult_202, 202, $"{TestDescription} | Notification: {TestJson}.");

            yield return ("#02", new ProcessingSkipped(s_successResult).AsResult_206, 206, $"{TestDescription} | Notification: {TestJson}.");

            yield return ("#03", new ProcessingFailed.Simplified(HttpStatusCode.PreconditionFailed, s_failedResult).AsResult_412, 412, $"{TestDescription} | Notification: {TestJson}.");
            yield return ("#04", new ProcessingFailed.Detailed(HttpStatusCode.PreconditionFailed, s_failedResult).AsResult_412, 412, $"{TestDescription} | Notification: {TestJson}.");

            yield return ("#05", new BadRequest.Simplified(s_failedResult).AsResult_400, 400, $"{CommonResources.Operation_ERROR_HttpRequest_Failure} | {TestDescription} | Notification: {TestJson}.");
            yield return ("#06", new BadRequest.Detailed(s_failedResult).AsResult_400, 400, $"{CommonResources.Operation_ERROR_HttpRequest_Failure} | {TestDescription} | Notification: {TestJson}.");
            
            yield return ("#07", new Forbidden.Simplified(s_failedResult).AsResult_403, 403, $"{CommonResources.Operation_ERROR_AccessDenied} | {TestDescription} | Notification: {TestJson}.");
            yield return ("#08", new Forbidden.Detailed(s_failedResult).AsResult_403, 403, $"{CommonResources.Operation_ERROR_AccessDenied} | {TestDescription} | Notification: {TestJson}.");
            
            yield return ("#09", new UnprocessableEntity.Simplified(s_failedResult).AsResult_422, 422, $"{CommonResources.Operation_ERROR_Deserialization_Failure} | {TestDescription} | Notification: {TestJson}.");
            yield return ("#10", new UnprocessableEntity.Detailed(s_failedResult).AsResult_422, 422, $"{CommonResources.Operation_ERROR_Deserialization_Failure} | {TestDescription} | Notification: {TestJson}.");
            
            yield return ("#11", new InternalError.Simplified(s_failedResult).AsResult_500, 500, $"{CommonResources.Operation_ERROR_Internal} | {TestDescription} | Notification: {TestJson}.");
            yield return ("#12", new InternalError.Detailed(s_failedResult).AsResult_500, 500, $"{CommonResources.Operation_ERROR_Internal} | {TestDescription} | Notification: {TestJson}.");
            
            yield return ("#13", new NotImplemented().AsResult_501, 501, CommonResources.Operation_ERROR_NotImplemented);
        }
    }
}