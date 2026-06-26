// © 2024, Worth Systems.

using NUnit.Framework;
using WebQueries.DataQuerying.Models.Responses;

namespace WebQueries.Tests.Unit.DataQuerying.Models
{
    [TestFixture]
    public sealed class HealthCheckResponseTests
    {
        [Test]
        public void Get_WhenSuccess_ReturnsNonEmptyString()
        {
            string result = HealthCheckResponse.Get(isSuccess: true);
            Assert.That(result, Is.Not.Empty);
        }

        [Test]
        public void Get_WhenFailure_ReturnsNonEmptyString()
        {
            string result = HealthCheckResponse.Get(isSuccess: false);
            Assert.That(result, Is.Not.Empty);
        }

        [Test]
        public void Get_SuccessAndFailureMessages_AreDifferent()
        {
            string success = HealthCheckResponse.Get(isSuccess: true);
            string failure = HealthCheckResponse.Get(isSuccess: false);
            Assert.That(success, Is.Not.EqualTo(failure));
        }
    }
}
