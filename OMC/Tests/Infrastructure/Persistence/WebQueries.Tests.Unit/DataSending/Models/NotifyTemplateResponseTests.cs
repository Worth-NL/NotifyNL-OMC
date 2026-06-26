// © 2024, Worth Systems.

using NUnit.Framework;
using WebQueries.DataSending.Models.Reponses;

namespace WebQueries.Tests.Unit.DataSending.Models
{
    [TestFixture]
    public sealed class NotifyTemplateResponseTests
    {
        private const string TestSubject = "Your case update";
        private const string TestBody = "Dear {{name}}, your case has been updated.";
        private const string TestError = "Template not found";

        #region Success()
        [Test]
        public void Success_IsSuccess_IsTrue()
        {
            NotifyTemplateResponse result = NotifyTemplateResponse.Success(TestSubject, TestBody);
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void Success_IsFailure_IsFalse()
        {
            NotifyTemplateResponse result = NotifyTemplateResponse.Success(TestSubject, TestBody);
            Assert.That(result.IsFailure, Is.False);
        }

        [Test]
        public void Success_Subject_IsProvidedValue()
        {
            NotifyTemplateResponse result = NotifyTemplateResponse.Success(TestSubject, TestBody);
            Assert.That(result.Subject, Is.EqualTo(TestSubject));
        }

        [Test]
        public void Success_Body_IsProvidedValue()
        {
            NotifyTemplateResponse result = NotifyTemplateResponse.Success(TestSubject, TestBody);
            Assert.That(result.Body, Is.EqualTo(TestBody));
        }

        [Test]
        public void Success_Error_IsEmpty()
        {
            NotifyTemplateResponse result = NotifyTemplateResponse.Success(TestSubject, TestBody);
            Assert.That(result.Error, Is.Empty);
        }
        #endregion

        #region Failure()
        [Test]
        public void Failure_IsSuccess_IsFalse()
        {
            NotifyTemplateResponse result = NotifyTemplateResponse.Failure(TestError);
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void Failure_IsFailure_IsTrue()
        {
            NotifyTemplateResponse result = NotifyTemplateResponse.Failure(TestError);
            Assert.That(result.IsFailure, Is.True);
        }

        [Test]
        public void Failure_Error_IsProvidedValue()
        {
            NotifyTemplateResponse result = NotifyTemplateResponse.Failure(TestError);
            Assert.That(result.Error, Is.EqualTo(TestError));
        }

        [Test]
        public void Failure_Subject_IsEmpty()
        {
            NotifyTemplateResponse result = NotifyTemplateResponse.Failure(TestError);
            Assert.That(result.Subject, Is.Empty);
        }

        [Test]
        public void Failure_Body_IsEmpty()
        {
            NotifyTemplateResponse result = NotifyTemplateResponse.Failure(TestError);
            Assert.That(result.Body, Is.Empty);
        }
        #endregion

        #region IsFailure is inverse of IsSuccess
        [Test]
        public void IsFailure_IsAlwaysInverseOfIsSuccess_ForSuccess()
        {
            NotifyTemplateResponse result = NotifyTemplateResponse.Success(TestSubject, TestBody);
            Assert.That(result.IsFailure, Is.EqualTo(!result.IsSuccess));
        }

        [Test]
        public void IsFailure_IsAlwaysInverseOfIsSuccess_ForFailure()
        {
            NotifyTemplateResponse result = NotifyTemplateResponse.Failure(TestError);
            Assert.That(result.IsFailure, Is.EqualTo(!result.IsSuccess));
        }
        #endregion
    }
}
