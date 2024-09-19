// © 2024, Worth Systems.

using EventsHandler.Extensions;

namespace EventsHandler.UnitTests.Extensions
{
    [TestFixture]
    public sealed class DateTimeExtensionsTests
    {
        [TestCaseSource(nameof(GetDateTimeCases))]
        public void ConvertToDutchDateString_LocalDutchDateTime_ReturnsExpectedLocalDutchDateString((string Id, DateTime DateTime, string ExpectedResult) test)
        {
            // Assert
            string actualResult = test.DateTime.ConvertToDutchDateString();

            // Assert
            Assert.That(actualResult, Is.EqualTo(test.ExpectedResult), message: $"Test {test.Id} failed: {test.DateTime}");
        }

        private static IEnumerable<(string Id, DateTime DateTime, string ExpectedResult)> GetDateTimeCases()
        {
            yield return ("#1", DateTime.MinValue, "0001-01-01");
            yield return ("#2", DateTime.MaxValue, "9999-12-31");
            yield return ("#3", new DateTime(2024, 09, 19, 14, 10, 00), "2024-09-19");
        }

        [Test]
        public void ConvertToDutchDateString_UtcDateTime_ReturnsExpectedLocalDutchDateString()
        {
            // Arrange
            DateTime utcTime = new(2024, 12, 31, 23, 00, 00, DateTimeKind.Utc);

            // Act
            string actualResult = utcTime.ConvertToDutchDateString();

            // Assert
            Assert.That(actualResult, Is.EqualTo("2025-01-01"));  // NOTE: UTC time is behind the local Dutch time
        }
    }
}