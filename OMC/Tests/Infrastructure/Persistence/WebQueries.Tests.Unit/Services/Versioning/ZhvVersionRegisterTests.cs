// © 2024, Worth Systems.

using Common.Versioning.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MoqExt;
using NUnit.Framework;
using WebQueries.DataQuerying.Strategies.Queries.Besluiten.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.Objecten.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.ObjectTypen.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenKlant.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenZaak.Interfaces;
using WebQueries.Register.Interfaces;
using WebQueries.Versioning;
using WebQueries.Versioning.Interfaces;

namespace WebQueries.Tests.Unit.Services.Versioning
{
    [TestFixture]
    public sealed class ZhvVersionRegisterTests
    {
        private const string TestVersion = "1.0.0";

        [Test]
        public void GetVersion_ForNotExistingServices_ReturnsEmptyString()
        {
            // Arrange
            IServiceProvider serviceProvider = new Mock<IServiceProvider>().Object;
            IVersionRegister register = new ZhvVersionRegister(serviceProvider);

            // Act
            string actualResult = register.GetVersion();

            // Assert
            Assert.That(actualResult, Is.Empty);
        }

        [Test]
        public void GetVersion_ForExistingServices_ReturnsExpectedString()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            MockQueryService<IQueryZaak>(serviceCollection);
            MockQueryService<IQueryKlant>(serviceCollection);
            MockQueryService<IQueryBesluiten>(serviceCollection);
            MockQueryService<IQueryObjecten>(serviceCollection);
            MockQueryService<IQueryObjectTypen>(serviceCollection);
            MockQueryService<ITelemetryService>(serviceCollection);

            IServiceProvider serviceProvider = new MockingContext(serviceCollection);
            IVersionRegister register = new ZhvVersionRegister(serviceProvider);

            // Act
            string actualResult = register.GetVersion();

            // Assert
            Assert.That(actualResult, Is.EqualTo(
                $"IQueryZaak v{TestVersion}, IQueryKlant v{TestVersion}, IQueryBesluiten v{TestVersion}, " +
                $"IQueryObjecten v{TestVersion}, IQueryObjectTypen v{TestVersion}, ITelemetryService v{TestVersion}"));
        }

        #region Helper methods
        private static void MockQueryService<TQueryService>(ServiceCollection serviceCollection)
            where TQueryService : class, IVersionDetails
        {
            var mockedQueryService = new Mock<TQueryService>();
            mockedQueryService.Setup(mock => mock.Name).Returns(typeof(TQueryService).Name);
            mockedQueryService.Setup(mock => mock.Version).Returns(TestVersion);

            serviceCollection.AddSingleton(mockedQueryService.Object);
        }
        #endregion
    }
}