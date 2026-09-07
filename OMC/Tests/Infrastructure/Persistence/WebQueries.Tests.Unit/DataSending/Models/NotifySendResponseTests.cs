// © 2024, Worth Systems.

using NUnit.Framework;
using WebQueries.DataSending.Models.Reponses;

namespace WebQueries.Tests.Unit.DataSending.Models
{
    [TestFixture]
    public sealed class NotifySendResponseTests
    {
        private const string TestError = "Something went wrong sending to Notify";

        #region Success()
        [Test]
        public void Success_IsSuccess_IsTrue()
        {
            NotifySendResponse result = NotifySendResponse.Success();
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void Success_IsFailure_IsFalse()
        {
            NotifySendResponse result = NotifySendResponse.Success();
            Assert.That(result.IsFailure, Is.False);
        }

        [Test]
        public void Success_Error_IsEmpty()
        {
            NotifySendResponse result = NotifySendResponse.Success();
            Assert.That(result.Error, Is.Empty);
        }
        #endregion

        #region Failure(string)
        [Test]
        public void Failure_IsSuccess_IsFalse()
        {
            NotifySendResponse result = NotifySendResponse.Failure(TestError);
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void Failure_IsFailure_IsTrue()
        {
            NotifySendResponse result = NotifySendResponse.Failure(TestError);
            Assert.That(result.IsFailure, Is.True);
        }

        [Test]
        public void Failure_Error_ContainsProvidedMessage()
        {
            NotifySendResponse result = NotifySendResponse.Failure(TestError);
            Assert.That(result.Error, Is.EqualTo(TestError));
        }
        #endregion

        #region Failure_Unknown()
        [Test]
        public void FailureUnknown_IsSuccess_IsFalse()
        {
            NotifySendResponse result = NotifySendResponse.Failure_Unknown();
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void FailureUnknown_IsFailure_IsTrue()
        {
            NotifySendResponse result = NotifySendResponse.Failure_Unknown();
            Assert.That(result.IsFailure, Is.True);
        }

        [Test]
        public void FailureUnknown_Error_IsNotEmpty()
        {
            NotifySendResponse result = NotifySendResponse.Failure_Unknown();
            Assert.That(result.Error, Is.Not.Empty);
        }
        #endregion

        #region IsFailure is inverse of IsSuccess
        [Test]
        public void IsFailure_IsAlwaysInverseOfIsSuccess_ForSuccess()
        {
            NotifySendResponse result = NotifySendResponse.Success();
            Assert.That(result.IsFailure, Is.EqualTo(!result.IsSuccess));
        }

        [Test]
        public void IsFailure_IsAlwaysInverseOfIsSuccess_ForFailure()
        {
            NotifySendResponse result = NotifySendResponse.Failure(TestError);
            Assert.That(result.IsFailure, Is.EqualTo(!result.IsSuccess));
        }
        #endregion
    }
}
