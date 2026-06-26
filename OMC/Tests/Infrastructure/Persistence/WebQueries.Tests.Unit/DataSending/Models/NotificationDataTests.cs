// © 2024, Worth Systems.

using NUnit.Framework;
using WebQueries.DataSending.Models.Reponses;

namespace WebQueries.Tests.Unit.DataSending.Models
{
    [TestFixture]
    public sealed class NotificationDataTests
    {
        private const string TestError = "Notification API returned 500";

        #region Failure()
        [Test]
        public void Failure_IsSuccess_IsFalse()
        {
            NotificationData result = NotificationData.Failure(TestError);
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void Failure_Error_IsProvidedMessage()
        {
            NotificationData result = NotificationData.Failure(TestError);
            Assert.That(result.Error, Is.EqualTo(TestError));
        }

        [Test]
        public void Failure_AllStringFields_AreEmpty()
        {
            NotificationData result = NotificationData.Failure(TestError);

            Assert.Multiple(() =>
            {
                Assert.That(result.Id, Is.Empty);
                Assert.That(result.CompletedAt, Is.Empty);
                Assert.That(result.EmailAddress, Is.Empty);
                Assert.That(result.Body, Is.Empty);
                Assert.That(result.Subject, Is.Empty);
                Assert.That(result.PhoneNumber, Is.Empty);
                Assert.That(result.Status, Is.Empty);
            });
        }
        #endregion
    }
}
