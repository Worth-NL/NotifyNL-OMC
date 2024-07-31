// © 2024, Worth Systems.

using EventsHandler.Properties;
using EventsHandler.Services.Settings;
using EventsHandler.Services.Settings.Enums;
using EventsHandler.Services.Settings.Strategy.Interfaces;
using EventsHandler.Services.Settings.Strategy.Manager;
using EventsHandler.Utilities._TestHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace EventsHandler.UnitTests.Services.Settings.Strategy.Manager
{
    [TestFixture]
    public class LoadersContextTests
    {
        private ILoadersContext? _loadersContext;

        [OneTimeSetUp]
        public void SetupTests()
        {
            // Service Provider
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(new AppSettingsLoader(ConfigurationHandler.GetConfiguration()));
            serviceCollection.AddSingleton(new EnvironmentLoader());
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            // Loaders context
            this._loadersContext = new LoadersContext(serviceProvider);
        }

        #region SetLoader
        [Test]
        public void SetLoader_ForInvalidEnum_ThrowsNotImplementedException()
        {
            // Arrange
            const LoaderTypes invalidType = (LoaderTypes)2;

            // Act & Assert
            Assert.Multiple(() =>
            {
                NotImplementedException? exception =
                    Assert.Throws<NotImplementedException>(() => this._loadersContext!.SetLoader(invalidType));

                Assert.That(exception?.Message, Is.EqualTo(Resources.Configuration_ERROR_Loader_NotImplemented));
            });
        }
        #endregion

        #region GetPathWithNode
        // IConfiguration
        [TestCase(LoaderTypes.AppSettings, "", "", "")]
        [TestCase(LoaderTypes.AppSettings, "abc", "", "abc")]
        [TestCase(LoaderTypes.AppSettings, "Path", "Node", "Path:Node")]
        [TestCase(LoaderTypes.AppSettings, "Path:Node", "SubNode", "Path:Node:SubNode")]
        // Environment variables
        [TestCase(LoaderTypes.Environment, "", "", "")]
        [TestCase(LoaderTypes.Environment, "abc", "", "ABC")]
        [TestCase(LoaderTypes.Environment, "ABC", "", "ABC")]
        [TestCase(LoaderTypes.Environment, "Path", "Node", "PATH_NODE")]
        [TestCase(LoaderTypes.Environment, "PATH", "NODE", "PATH_NODE")]
        [TestCase(LoaderTypes.Environment, "Path_Node", "SubNode", "PATH_NODE_SUBNODE")]
        [TestCase(LoaderTypes.Environment, "PAtH_NoDe", "SubNODE", "PATH_NODE_SUBNODE")]
        [TestCase(LoaderTypes.Environment, "PATH_NODE", "SUBNODE", "PATH_NODE_SUBNODE")]
        public void GetPathWithNode_WithLoadingService_ForGivenPathsAndNodes_ReturnsExpectedPath(
            LoaderTypes loaderType, string testCurrentPath, string testNodeName, string expectedPath)
        {
            // Arrange
            this._loadersContext!.SetLoader(loaderType);

            // Act
            string actualPath = this._loadersContext!.GetPathWithNode(testCurrentPath, testNodeName);

            // Assert
            Assert.That(actualPath, Is.EqualTo(expectedPath));
        }
        #endregion
    }
}
