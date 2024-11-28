// © 2024, Worth Systems.

using Common.Properties;
using Common.Settings;
using Common.Settings.Interfaces;
using Common.Tests.Utilities._TestHelpers;
using NUnit.Framework;

namespace Common.Tests.Unit.Settings
{
    [TestFixture]
    public sealed class AppSettingsLoaderTests
    {
        private readonly ILoadingService _loadingService = new AppSettingsLoader(ConfigurationHandler.GetConfiguration());

        #region GetData<T>
        [TestCase("")]
        [TestCase(" ")]
        public void GetData_ForEmptyKey_ThrowsKeyNotFoundException(string emptyKey)
        {
            // Act & Asser
            Assert.Multiple(() =>
            {
                KeyNotFoundException? exception = Assert.Throws<KeyNotFoundException>(() =>
                    this._loadingService.GetData<string>(emptyKey, disableValidation: false));
                Assert.That(exception?.Message, Is.EqualTo(AppResources.Configuration_ERROR_InvalidKey));
            });
        }
        #endregion

        #region GetPathWithNode
        // Both sides empty
        [TestCase("", "", "")]
        [TestCase("", " ", "")]
        [TestCase(" ", "", "")]
        [TestCase(" ", " ", "")]
        // Left side empty
        [TestCase("", "node", "")]
        [TestCase(" ", "node", "")]
        // Right side empty
        [TestCase("path", "", "path")]
        [TestCase("path", " ", "path")]
        // Skip "AppSettings"
        [TestCase("AppSettings", "node", "node")]
        // Valid case
        [TestCase("path", "node", "path:node")]
        public void GetPathWithNode_ForGivenValues_ReturnsExpectedResult(string testPath, string testNodeName, string expectedResult)
        {
            // Act
            string actualResult = this._loadingService.GetPathWithNode(testPath, testNodeName);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
        #endregion

        #region GetNodePath
        [TestCase("", "")]
        [TestCase(" ", "")]
        [TestCase("node", ":node")]
        public void GetNodePath_ForGivenValues_ReturnsExpectedResult(string testNodeName, string expectedResult)
        {
            // Act
            string actualResult = this._loadingService.GetNodePath(testNodeName);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
        #endregion
    }
}