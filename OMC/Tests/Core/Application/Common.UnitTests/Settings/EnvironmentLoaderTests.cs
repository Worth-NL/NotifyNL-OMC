// © 2024, Worth Systems.

using Common.Properties;
using Common.Settings;
using Common.Settings.DAO.Interfaces;
using Common.Settings.Interfaces;
using Moq;
using NUnit.Framework;

namespace Common.Tests.Unit.Settings
{
    [TestFixture]
    public sealed class EnvironmentLoaderTests
    {
        private readonly ILoadingService _loadingService = new EnvironmentLoader
        {
            Environment = new Mock<IEnvironment>().Object
        };

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
        [TestCase("path", "", "PATH")]
        [TestCase("path", " ", "PATH")]
        // Valid case
        [TestCase("path", "node", "PATH_NODE")]
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
        [TestCase("node", "_NODE")]
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