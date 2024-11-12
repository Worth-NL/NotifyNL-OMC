// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Strategy.Responses;
using EventsHandler.Services.Responding;
using EventsHandler.Services.Responding.Interfaces;
using EventsHandler.Services.Responding.Messages.Models.Details;
using EventsHandler.Services.Responding.Messages.Models.Details.Base;
using EventsHandler.Services.Responding.Results.Builder.Interface;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace EventsHandler.UnitTests.Services.Responding
{
    [TestFixture]
    public sealed class OmcResponderTests
    {
        #region Test data
        private const string TestDescription = "Test description";
        private const string TestJson = "{ }";
        private const string TestMessage = "Test message";
        private const string TestCases = "Case 1, Case 2";
        private static readonly string[] s_testReasons = { "Reason 1, Reason2, Reason 3" };

        private static readonly InfoDetails s_infoDetails = new(TestMessage, TestCases, s_testReasons);
        private static readonly ErrorDetails s_errorDetails = new(TestMessage, TestCases, s_testReasons);
        private static readonly ErrorDetails s_httpErrorDetails = new(Resources.HttpRequest_ERROR_NoCase, TestCases, s_testReasons);
        private static readonly SimpleDetails s_simpleDetails = new(TestMessage);
        #endregion

        private readonly Mock<IDetailsBuilder> _mockedBuilder = new(MockBehavior.Strict);

        [TestCaseSource(nameof(GetTestResults))]
        public void GetResponse_ProcessingResult_Success_ReturnsExpectedObjectResult(
            (string Id, ProcessingResult Result, ProcessingStatus Status, HttpStatusCode Code, int ObjResultCode, string ObjResultName, string Description, string Content) test)
        {
            // Arrange
            IRespondingService<ProcessingResult> omcResponder = new OmcResponder(this._mockedBuilder.Object);

            // Act
            ObjectResult actualResponse = omcResponder.GetResponse(test.Result);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualResponse.StatusCode, Is.EqualTo(test.ObjResultCode), FailedTestMessage(test.Id));
                Assert.That(actualResponse.Value?.ToString(), 
                    Is.EqualTo($"{(int)test.Code} {test.Code} | {test.Description} | {test.ObjResultName} {test.Content}"), FailedTestMessage(test.Id));
            });

            return;

            static string FailedTestMessage(string id) => $"Test {id} failed.";
        }

        private static IEnumerable<
            (string Id, ProcessingResult Result, ProcessingStatus Status, HttpStatusCode Code, int ObjResultCode, string ObjResultName, string Description, string Content)> GetTestResults()
        {
            yield return ("#1", ProcessingResult.Success(TestDescription, TestJson, s_infoDetails), ProcessingStatus.Success, HttpStatusCode.Accepted, 202, nameof(SimpleDetails), $"{TestDescription} | Notification: {TestJson}.", $"{{ Message = {TestMessage} }}");
            yield return ("#2", ProcessingResult.Skipped(TestDescription, TestJson, s_infoDetails), ProcessingStatus.Skipped, HttpStatusCode.PartialContent, 206, nameof(SimpleDetails), $"{TestDescription} | Notification: {TestJson}.", $"{{ Message = {TestMessage} }}");
            yield return ("#3", ProcessingResult.Aborted(TestDescription, TestJson, s_errorDetails), ProcessingStatus.Aborted, HttpStatusCode.PartialContent, 206, nameof(SimpleDetails), $"{TestDescription} | Notification: {TestJson}.", $"{{ Message = {TestMessage} }}");
            yield return ("#4", ProcessingResult.NotPossible(TestDescription, TestJson, s_errorDetails), ProcessingStatus.NotPossible, HttpStatusCode.UnprocessableEntity, 206, nameof(ErrorDetails), $"{Resources.Operation_ERROR_Deserialization_Failure} | {TestDescription} | Notification: {TestJson}.", $"{{ Message = {TestMessage}, Cases = {TestCases}, Reasons = {s_testReasons} }}");
            yield return ("#5", ProcessingResult.Failure(TestDescription, TestJson, s_httpErrorDetails), ProcessingStatus.Failure, HttpStatusCode.BadRequest, 400, nameof(SimpleDetails), $"{Resources.Operation_ERROR_HttpRequest_Failure} | {TestDescription} | Notification: {TestJson}.", $"{{ Message = {Resources.HttpRequest_ERROR_NoCase} }}");
            yield return ("#6", ProcessingResult.Failure(TestDescription, TestJson, s_errorDetails), ProcessingStatus.Failure, HttpStatusCode.PreconditionFailed, 412, nameof(ErrorDetails), $"{TestDescription} | Notification: {TestJson}.", $"{{ Message = {TestMessage}, Cases = {TestCases}, Reasons = {s_testReasons} }}");
            yield return ("#7", ProcessingResult.Unknown(TestDescription, TestJson, s_simpleDetails), ProcessingStatus.Failure, HttpStatusCode.PreconditionFailed, 412, nameof(SimpleDetails), $"{TestDescription} | Notification: {TestJson}.", $"{{ Message = {TestMessage} }}");
        }
    }
}