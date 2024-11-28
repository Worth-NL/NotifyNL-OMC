// © 2023, Worth Systems.

using Common.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace EventsHandler.Tests.Unit.Extensions
{
    [TestFixture]
    public sealed class ServiceCollectionExtensionsTests
    {
        private IServiceCollection? _serviceCollection;

        #region Test data
        private sealed class TestService;
        #endregion

        [SetUp]
        public void InitializeTests()
        {
            this._serviceCollection = new ServiceCollection();
        }

        [Test]
        public void GetRequiredService_ForExistingService_ReturnsTheService()
        {
            // Arrange
            this._serviceCollection!.AddSingleton<TestService>();

            // Act
            TestService actualResult = this._serviceCollection!.GetRequiredService<TestService>();

            // Assert
            Assert.That(actualResult, Is.Not.Null);
        }

        [Test]
        public void GetRequiredService_ForNotExistingService_ThrowsInvalidOperationException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => this._serviceCollection!.GetRequiredService<TestService>());
        }
    }
}