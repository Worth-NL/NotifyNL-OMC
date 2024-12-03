// © 2024, Worth Systems.

using Common.Extensions;

namespace EventsHandler.Tests.Unit.Extensions
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
            yield return ("#1", DateTime.MinValue, "01-01-0001");
            yield return ("#2", DateTime.MaxValue, "31-12-9999");
            yield return ("#3", new DateTime(2024, 09, 19, 14, 10, 00), "19-09-2024");
        }

        [Test]
        public void ConvertToDutchDateString_UtcDateTime_ReturnsExpectedLocalDutchDateString()
        {
            // Arrange
            DateTime utcTime = new(2024, 12, 31, 23, 00, 00, DateTimeKind.Utc);

            // Act
            string actualResult = utcTime.ConvertToDutchDateString();

            // Assert
            Assert.That(actualResult, Is.EqualTo("01-01-2025"));  // NOTE: UTC time is behind the local Dutch time
        }

        [Test]
        public void ConvertToDutchDateString_DateOnly_ReturnsExpectedLocalDutchDateString()
        {
            // Arrange
            DateOnly date = new(2024, 12, 31);

            // Act
            string actualResult = date.ConvertToDutchDateString();

            // Assert
            Assert.That(actualResult, Is.EqualTo("31-12-2024"));
        }
    }
}