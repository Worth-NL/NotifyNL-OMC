﻿// © 2024, Worth Systems.

using EventsHandler.Services.DataLoading;
using EventsHandler.Services.DataLoading.Enums;
using EventsHandler.Services.DataLoading.Strategy.Interfaces;
using EventsHandler.Services.DataLoading.Strategy.Manager;
using EventsHandler.Utilities._TestHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace EventsHandler.UnitTests.Services.DataLoading
{
    [TestFixture]
    public class LoadersContextTests
    {
        private ILoadersContext? _loadersContext;

        [OneTimeSetUp]
        public void SetupTests()
        {
            // Service Provider (does not require mocking)
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(new ConfigurationLoader(ConfigurationHandler.GetConfiguration()));
            serviceCollection.AddSingleton(new EnvironmentLoader());
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            // Loaders context
            this._loadersContext = new LoadersContext(serviceProvider);
        }

        [TearDown]
        public void ResetTests()
        {
            this._loadersContext?.Dispose();
        }
        
        [OneTimeTearDown]
        public void CleanupTests()
        {
            ResetTests();
        }
        
        #region SetLoader
        [Test]
        public void SetLoader_ForInvalidEnum_ThrowsNotImplementedException()
        {
            // Arrange
            const LoaderTypes invalidType = (LoaderTypes)2;

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => this._loadersContext!.SetLoader(invalidType));
        }
        #endregion

        #region GetData
        [Test]
        public void GetData_WithoutLoadingService_ThrowsNotImplementedException()
        {
            // Act & Assert
            Assert.Throws<NotImplementedException>(() =>
                this._loadersContext!.GetData<string>(string.Empty));
        }
        #endregion

        #region GetPathWithNode
        [Test]
        public void GetPathWithNode_WithoutLoadingService_ThrowsNotImplementedException()
        {
            // Act & Assert
            Assert.Throws<NotImplementedException>(() =>
                this._loadersContext!.GetPathWithNode(string.Empty, string.Empty));
        }
        
        // IConfiguration
        [TestCase(LoaderTypes.Configuration, "", "", "")]
        [TestCase(LoaderTypes.Configuration, "abc", "", "abc")]
        [TestCase(LoaderTypes.Configuration, "Path", "Node", "Path:Node")]
        [TestCase(LoaderTypes.Configuration, "Path:Node", "SubNode", "Path:Node:SubNode")]
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
