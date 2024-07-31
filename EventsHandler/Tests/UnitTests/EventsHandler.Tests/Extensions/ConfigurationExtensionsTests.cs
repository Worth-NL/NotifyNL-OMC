// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Extensions;
using EventsHandler.Properties;
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

        #region ValidateNotEmpty<T>
        [TestCase("")]
        public void ValidateNotEmpty_ForInvalidValue_ThrowsArgumentException(string testValue)
        {
            // Act & Assert
            Assert.Multiple(() =>
            {
                ArgumentException? exception = Assert.Throws<ArgumentException>(() => testValue.ValidateNotEmpty(testValue));
                Assert.That(exception?.Message, Is.EqualTo(Resources.Configuration_ERROR_ValueNotFoundOrEmpty
                                                  .Replace("{0}", testValue)));
            });
        }

        [Test]
        public void ValidateNotEmpty_ForValidValue_ReturnsExpectedValue()
        {
            // Arrange
            const string testValue = "Valid";

            // Act
            string actualValue = testValue.ValidateNotEmpty(testValue);

            // Assert
            Assert.That(actualValue, Is.EqualTo(testValue));
        }
        #endregion

        #region ValidateNoHttp
        [TestCase("http://google.com/")]
        [TestCase("https://google.com/")]
        public void ValidateNoHttp_ForInvalidValue_ThrowsArgumentException(string testUrl)
        {
            // Act & Assert
            Assert.Multiple(() =>
            {
                ArgumentException? exception = Assert.Throws<ArgumentException>(() => testUrl.ValidateNoHttp());
                Assert.That(exception?.Message, Is.EqualTo(Resources.Configuration_ERROR_ContainsHttp
                                                  .Replace("{0}", testUrl)));
            });
        }

        [Test]
        public void ValidateNoHttp_ForValidValue_ReturnsOriginalValue()
        {
            // Arrange
            const string testUrl = "www.google.com";

            // Act
            string actualResult = testUrl.ValidateNoHttp();

            // Assert
            Assert.That(actualResult, Is.EqualTo(testUrl));
        }
        #endregion

        #region ValidateNoEndpoint
        [Test]
        public void ValidateNoEndpoint_ForInvalidValue_ThrowsArgumentException()
        {
            // Arrange
            const string testUrl = "testUrl/api/endpoint";

            // Act & Assert
            Assert.Multiple(() =>
            {
                ArgumentException? exception = Assert.Throws<ArgumentException>(() => testUrl.ValidateNoEndpoint());
                Assert.That(exception?.Message, Is.EqualTo(Resources.Configuration_ERROR_ContainsEndpoint
                                                  .Replace("{0}", testUrl)));
            });
        }

        [Test]
        public void ValidateNoEndpoint_ForValidValue_ReturnsOriginalValue()
        {
            // Arrange
            const string testUrl = "testUrl";

            // Act
            string actualResult = testUrl.ValidateNoEndpoint();

            // Assert
            Assert.That(actualResult, Is.EqualTo(testUrl));
        }
        #endregion

        #region ValidateTemplateId
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
        public void ValidateTemplateId_ForInvalidValue_ThrowsArgumentException(string testTemplateId)
        {
            // Act & Assert
            Assert.Multiple(() =>
            {
                ArgumentException? exception = Assert.Throws<ArgumentException>(() => testTemplateId.ValidateTemplateId());
                Assert.That(exception?.Message, Is.EqualTo(Resources.Configuration_ERROR_InvalidTemplateId
                                                  .Replace("{0}", testTemplateId)));
            });
        }

        [Test]
        public void ValidTemplateId_ForValidValue_ReturnsOriginalValue()
        {
            // Arrange
            const string testTemplateId = "abcdefgh-1234-abcd-abcd-abcdefghijkl";

            // Act
            string actualResult = testTemplateId.ValidateTemplateId();

            // Assert
            Assert.That(actualResult, Is.EqualTo(testTemplateId));
        }
        #endregion

        #region ValidateUri
        [Test]
        public void ValidateUri_ForInvalidValue_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Multiple(() =>
            {
                ArgumentException? exception = Assert.Throws<ArgumentException>(() => DefaultValues.Models.EmptyUri.ValidateUri());
                Assert.That(exception?.Message, Is.EqualTo(Resources.Configuration_ERROR_InvalidUri
                                                  .Replace("{0}", DefaultValues.Models.EmptyUri.ToString())));
            });
        }

        [Test]
        public void ValidateUri_ForValidValue_ReturnsOriginalValue()
        {
            // Arrange
            var testUri = new Uri("https://www.bing.com/");

            // Act
            Uri actualResult = testUri.ValidateUri();

            // Assert
            Assert.That(actualResult, Is.EqualTo(testUri));
        }
        #endregion
    }
}
