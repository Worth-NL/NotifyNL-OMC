// © 2024, Worth Systems.

using NUnit.Framework;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataSending.Models.DTOs;
using ZgwModels.Enums;

namespace WebQueries.Tests.Unit.DataQuerying.Models
{
    [TestFixture]
    public sealed class QueryingDataResponseTests
    {
        private static readonly IReadOnlyCollection<NotifyData> s_content =
        [
            new NotifyData(NotifyMethods.Email),
            new NotifyData(NotifyMethods.Sms)
        ];

        #region Success()
        [Test]
        public void Success_IsSuccess_IsTrue()
        {
            QueryingDataResponse result = QueryingDataResponse.Success(s_content);
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void Success_IsFailure_IsFalse()
        {
            QueryingDataResponse result = QueryingDataResponse.Success(s_content);
            Assert.That(result.IsFailure, Is.False);
        }

        [Test]
        public void Success_Message_IsNotEmpty()
        {
            QueryingDataResponse result = QueryingDataResponse.Success(s_content);
            Assert.That(result.Message, Is.Not.Empty);
        }

        [Test]
        public void Success_Content_MatchesProvidedCollection()
        {
            QueryingDataResponse result = QueryingDataResponse.Success(s_content);
            Assert.That(result.Content, Is.EqualTo(s_content));
        }
        #endregion

        #region Failure()
        [Test]
        public void Failure_IsSuccess_IsFalse()
        {
            QueryingDataResponse result = QueryingDataResponse.Failure();
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void Failure_IsFailure_IsTrue()
        {
            QueryingDataResponse result = QueryingDataResponse.Failure();
            Assert.That(result.IsFailure, Is.True);
        }

        [Test]
        public void Failure_Message_IsNotEmpty()
        {
            QueryingDataResponse result = QueryingDataResponse.Failure();
            Assert.That(result.Message, Is.Not.Empty);
        }

        [Test]
        public void Failure_Content_IsEmpty()
        {
            QueryingDataResponse result = QueryingDataResponse.Failure();
            Assert.That(result.Content, Is.Empty);
        }
        #endregion

        #region IsFailure is inverse of IsSuccess
        [Test]
        public void IsFailure_IsAlwaysInverseOfIsSuccess_ForSuccess()
        {
            QueryingDataResponse result = QueryingDataResponse.Success(s_content);
            Assert.That(result.IsFailure, Is.EqualTo(!result.IsSuccess));
        }

        [Test]
        public void IsFailure_IsAlwaysInverseOfIsSuccess_ForFailure()
        {
            QueryingDataResponse result = QueryingDataResponse.Failure();
            Assert.That(result.IsFailure, Is.EqualTo(!result.IsSuccess));
        }
        #endregion
    }
}
