﻿// © 2024, Worth Systems.

using Common.Constants;
using Common.Properties;
using Common.Settings.Configuration;
using Common.Settings.Extensions;
using Common.Tests.Utilities._TestHelpers;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Common.Tests.Unit.Settings.Extensions
{
    [TestFixture]
    internal sealed class ConfigurationExtensionsTests
    {
        private IConfiguration _appSettingsConfiguration = null!;
        private OmcConfiguration _omcConfiguration = null!;

        [OneTimeSetUp]
        public void InitializeTests()
        {
            this._appSettingsConfiguration = ConfigurationHandler.GetConfiguration();
            this._omcConfiguration = ConfigurationHandler.GetOmcConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.ValidEnvironment_v1);
        }

        [OneTimeTearDown]
        public void CleanupTests()
        {
            this._omcConfiguration.Dispose();
        }

        #region Specific GetValue methods
        [Test]
        public void Encryption_ReturnsExpectedValue()
        {
            // Act
            bool actualValue = this._appSettingsConfiguration.IsEncryptionAsymmetric();

            // Assert
            Assert.That(actualValue, Is.False);
        }

        [Test]
        public void OpenZaakDomain_ReturnsExpectedValue()
        {
            // Act
            string actualValue = ConfigExtensions.OpenZaakDomain(this._omcConfiguration);

            // Assert
            Assert.That(actualValue, Is.Not.Empty);
        }
        #endregion

        #region GetNotEmpty<T>
        [TestCase("")]
        [TestCase(" ")]
        public void ValidateNotEmpty_ForInvalidValue_ThrowsArgumentException(string testValue)
        {
            // Act & Assert
            Assert.Multiple(() =>
            {
                ArgumentException? exception = Assert.Throws<ArgumentException>(() => testValue.GetNotEmpty(testValue));
                Assert.That(exception?.Message, Is.EqualTo(CommonResources.Configuration_ERROR_ValueNotFoundOrEmpty
                                                  .Replace("{0}", testValue)));
            });
        }

        [Test]
        public void ValidateNotEmpty_ForValidValue_ReturnsExpectedValue()
        {
            // Arrange
            const string testValue = "Valid";

            // Act
            string actualValue = testValue.GetNotEmpty(testValue);

            // Assert
            Assert.That(actualValue, Is.EqualTo(testValue));
        }
        #endregion

        #region GetWithoutProtocol
        [TestCase("http://google.com/")]
        [TestCase("https://google.com/")]
        public void ValidateNoHttp_ForInvalidValue_ThrowsArgumentException(string testUrl)
        {
            // Act & Assert
            Assert.Multiple(() =>
            {
                ArgumentException? exception = Assert.Throws<ArgumentException>(() => testUrl.GetWithoutProtocol());
                Assert.That(exception?.Message, Is.EqualTo(CommonResources.Configuration_ERROR_ContainsHttp
                                                  .Replace("{0}", testUrl)));
            });
        }

        [Test]
        public void ValidateNoHttp_ForValidValue_ReturnsOriginalValue()
        {
            // Arrange
            const string testUrl = "www.google.com";

            // Act
            string actualResult = testUrl.GetWithoutProtocol();

            // Assert
            Assert.That(actualResult, Is.EqualTo(testUrl));
        }
        #endregion

        #region GetValidGuid
        // ReSharper disable StringLiteralTypo
        [TestCase("")]
        [TestCase(" ")]
        // Not enough characters
        [TestCase("abcdef0-1234-abcd-abcd-abcdef123456")]
        [TestCase("abcdef01-123-abcd-abcd-abcdef123456")]
        [TestCase("abcdef01-1234-abc-abcd-abcdef123456")]
        [TestCase("abcdef01-1234-abcd-abc-abcdef123456")]
        [TestCase("abcdef01-1234-abcd-abcd-abcdef12345")]
        // Too many characters
        [TestCase("abcdef012-1234-abcd-abcd-abcdef123456")]
        [TestCase("abcdef01-12345-abcd-abcd-abcdef123456")]
        [TestCase("abcdef01-1234-abcde-abcd-abcdef123456")]
        [TestCase("abcdef01-1234-abcd-abcde-abcdef123456")]
        [TestCase("abcdef01-1234-abcd-abcd-abcdef1234567")]
        // Not HEX values
        [TestCase("abcdefg1-1234-abcd-abcd-abcdef123456")]  // "g" is invalid HEX (17th value)
        public void ValidateTemplateId_ForInvalidValue_ThrowsArgumentException(string testTemplateId)
        {
            // Act & Assert
            Assert.Multiple(() =>
            {
                ArgumentException? exception = Assert.Throws<ArgumentException>(() => testTemplateId.GetValidGuid());
                Assert.That(exception?.Message, Is.EqualTo(CommonResources.Configuration_ERROR_InvalidTemplateId
                                                  .Replace("{0}", testTemplateId)));
            });
        }

        [Test]
        public void ValidTemplateId_ForValidValue_ReturnsOriginalValue()
        {
            // Arrange
            const string testTemplateId = "00abcdef-1234-abcd-abcd-abcdef123456";  // HEX values: 0-f

            // Act
            Guid actualResult = testTemplateId.GetValidGuid();

            // Assert
            Guid expectedResult = new(testTemplateId);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
        #endregion

        #region GetValidUri
        [Test]
        public void ValidateUri_ForInvalidValue_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Multiple(() =>
            {
                ArgumentException? exception = Assert.Throws<ArgumentException>(() => CommonValues.Default.Models.EmptyUri.ToString().GetValidUri());
                Assert.That(exception?.Message, Is.EqualTo(CommonResources.Configuration_ERROR_InvalidUri
                                                  .Replace("{0}", CommonValues.Default.Models.EmptyUri.ToString())));
            });
        }

        [Test]
        public void ValidateUri_ForValidValue_ReturnsOriginalValue()
        {
            // Arrange
            const string testUri = "https://www.bing.com/";

            // Act
            Uri actualResult = testUri.GetValidUri();

            // Assert
            var expectedResult = new Uri(testUri);
            
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
        #endregion
    }
}
