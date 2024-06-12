// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Utilities._TestHelpers;
using Microsoft.Extensions.Configuration;

namespace EventsHandler.UnitTests.Extensions
{
    [TestFixture]
    internal sealed class ConfigurationExtensionsTests
    {
        private IConfiguration? _configuration;

        [OneTimeSetUp]
        public void InitializeTests()
        {
            this._configuration = ConfigurationHandler.GetConfiguration();
        }

        #region Specific GetValue methods
        [Test]
        public void Encryption_ReturnsExpectedValue()
        {
            // Act
            bool actualValue = this._configuration!.IsEncryptionAsymmetric();

            // Assert
            Assert.That(actualValue, Is.False);
        }
        
        [Test]
        public void Features_ReturnsExpectedValue()
        {
            // Act
            int actualValue = this._configuration!.OmcWorkflowVersion();

            // Assert
            Assert.That(actualValue, Is.EqualTo(1));
        }
        #endregion

        #region NotEmpty<T>
        [Test]
        public void GetConfigValue_Generic_ForExistingPathOrValue_ReturnsExpectedValue()
        {
            // Arrange
            const string testValue = "Valid";

            // Act
            string actualValue = testValue.NotEmpty(testValue);

            // Assert
            Assert.That(actualValue, Is.EqualTo(testValue));
        }

        [TestCase("")]
        public void GetConfigValue_Generic_ForNotExistingPathOrValue_ThrowsArgumentException(string testValue)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => testValue.NotEmpty(testValue));
        }
        #endregion

        #region WithoutHttp
        [TestCase("http://google.com/")]
        [TestCase("https://google.com/")]
        public void WithoutHttp_ForInvalidValue_ThrowsArgumentException(string testUrl)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => testUrl.WithoutHttp());
        }

        [Test]
        public void WithoutHttp_ForValidValue_ReturnsOriginalValue()
        {
            // Arrange
            const string testUrl = "www.google.com";

            // Act
            string actualResult = testUrl.WithoutHttp();

            // Assert
            Assert.That(actualResult, Is.EqualTo(testUrl));
        }
        #endregion

        #region WithoutEndpoint
        [Test]
        public void WithoutEndpoint_ForValidValue_ThrowsArgumentException()
        {
            // Arrange
            const string testUrl = "testUrl/api/endpoint";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => testUrl.WithoutEndpoint());
        }

        [Test]
        public void WithoutEndpoint_ForInvalidValue_ReturnsOriginalValue()
        {
            // Arrange
            const string testUrl = "testUrl";

            // Act
            string actualResult = testUrl.WithoutEndpoint();

            // Assert
            Assert.That(actualResult, Is.EqualTo(testUrl));
        }
        #endregion

        #region WithoutEndpoint
        // ReSharper disable StringLiteralTypo
        [TestCase("")]
        [TestCase(" ")]
        // Not enough characters
        [TestCase("abcdefg-1234-abcd-abcd-abcdefghijkl")]
        [TestCase("abcdefgh-123-abcd-abcd-abcdefghijkl")]
        [TestCase("abcdefgh-1234-abc-abcd-abcdefghijkl")]
        [TestCase("abcdefgh-1234-abcd-abc-abcdefghijkl")]
        [TestCase("abcdefgh-1234-abcd-abcd-abcdefghijk")]
        // Too many characters
        [TestCase("abcdefghi-1234-abcd-abcd-abcdefghijkl")]
        [TestCase("abcdefgh-12345-abcd-abcd-abcdefghijkl")]
        [TestCase("abcdefgh-1234-abcde-abcd-abcdefghijkl")]
        [TestCase("abcdefgh-1234-abcd-abcde-abcdefghijkl")]
        [TestCase("abcdefgh-1234-abcd-abcd-abcdefghijklm")]
        public void ValidTemplateId_ForValidValue_ThrowsArgumentException(string testTemplateId)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => testTemplateId.ValidTemplateId());
        }

        [Test]
        public void ValidTemplateId_ForInvalidValue_ReturnsOriginalValue()
        {
            // Arrange
            const string testTemplateId = "abcdefgh-1234-abcd-abcd-abcdefghijkl";

            // Act
            string actualResult = testTemplateId.ValidTemplateId();

            // Assert
            Assert.That(actualResult, Is.EqualTo(testTemplateId));
        }
        #endregion
    }
}
