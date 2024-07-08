// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Extensions;

namespace EventsHandler.UnitTests.Extensions
{
    [TestFixture]
    public sealed class UriExtensionsTests
    {
        [Test]
        public void GetGuid_ForMissingUri_ReturnsEmptyGuid()
        {
            // Act
            Guid actualGuid = UriExtensions.GetGuid(null);

            // Assert
            Assert.That(actualGuid, Is.Default);
        }
        
        [Test]
        public void GetGuid_ForDefaultUri_ReturnsEmptyGuid()
        {
            // Act
            Guid actualGuid = DefaultValues.Models.EmptyUri.GetGuid();

            // Assert
            Assert.That(actualGuid, Is.Default);
        }

        [Test]
        public void GetGuid_ForInvalidUri_ReturnsEmptyGuid()
        {
            // Act
            Guid actualGuid = new Uri("https://www.google.com/").GetGuid();

            // Assert
            Assert.That(actualGuid, Is.Default);
        }
        
        [Test]
        public void GetGuid_ForValidUri_ReturnsExtractedGuid()
        {
            // Arrange
            var expectedGuid = new Guid("12345678-1234-1234-1234-123456789012");
            
            // Act
            Guid actualGuid = new Uri($"https://www.google.com/{expectedGuid}").GetGuid();

            // Assert
            Assert.That(actualGuid, Is.EqualTo(expectedGuid));
        }
    }
}