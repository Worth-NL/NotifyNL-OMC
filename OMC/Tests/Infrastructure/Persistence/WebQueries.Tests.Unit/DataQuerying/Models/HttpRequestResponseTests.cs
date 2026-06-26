// © 2024, Worth Systems.

using NUnit.Framework;
using WebQueries.DataQuerying.Models.Responses;

namespace WebQueries.Tests.Unit.DataQuerying.Models
{
    [TestFixture]
    public sealed class HttpRequestResponseTests
    {
        private const string TestJson = "{ \"status\": \"ok\" }";
        private const string TestError = "Something went wrong";

        #region Success()
        [Test]
        public void Success_IsSuccess_IsTrue()
        {
            HttpRequestResponse result = HttpRequestResponse.Success(TestJson);
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void Success_IsFailure_IsFalse()
        {
            HttpRequestResponse result = HttpRequestResponse.Success(TestJson);
            Assert.That(result.IsFailure, Is.False);
        }

        [Test]
        public void Success_JsonResponse_ContainsProvidedJson()
        {
            HttpRequestResponse result = HttpRequestResponse.Success(TestJson);
            Assert.That(result.JsonResponse, Is.EqualTo(TestJson));
        }
        #endregion

        #region Failure()
        [Test]
        public void Failure_IsSuccess_IsFalse()
        {
            HttpRequestResponse result = HttpRequestResponse.Failure(TestError);
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void Failure_IsFailure_IsTrue()
        {
            HttpRequestResponse result = HttpRequestResponse.Failure(TestError);
            Assert.That(result.IsFailure, Is.True);
        }

        [Test]
        public void Failure_JsonResponse_ContainsProvidedMessage()
        {
            HttpRequestResponse result = HttpRequestResponse.Failure(TestError);
            Assert.That(result.JsonResponse, Is.EqualTo(TestError));
        }
        #endregion

        #region IsFailure is inverse of IsSuccess
        [Test]
        public void IsFailure_IsAlwaysInverseOfIsSuccess_ForSuccess()
        {
            HttpRequestResponse result = HttpRequestResponse.Success(TestJson);
            Assert.That(result.IsFailure, Is.EqualTo(!result.IsSuccess));
        }

        [Test]
        public void IsFailure_IsAlwaysInverseOfIsSuccess_ForFailure()
        {
            HttpRequestResponse result = HttpRequestResponse.Failure(TestError);
            Assert.That(result.IsFailure, Is.EqualTo(!result.IsSuccess));
        }
        #endregion
    }
}
