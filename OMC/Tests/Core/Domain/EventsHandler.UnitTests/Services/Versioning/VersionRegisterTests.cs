// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using EventsHandler.Services.DataQuerying.Strategies.Queries.Besluiten.Interfaces;
using EventsHandler.Services.DataQuerying.Strategies.Queries.Besluiten.v1;
using EventsHandler.Services.DataQuerying.Strategies.Queries.Objecten.Interfaces;
using EventsHandler.Services.DataQuerying.Strategies.Queries.Objecten.v1;
using EventsHandler.Services.DataQuerying.Strategies.Queries.ObjectTypen.Interfaces;
using EventsHandler.Services.DataQuerying.Strategies.Queries.ObjectTypen.v1;
using EventsHandler.Services.DataQuerying.Strategies.Queries.OpenKlant.Interfaces;
using EventsHandler.Services.DataQuerying.Strategies.Queries.OpenKlant.v2;
using EventsHandler.Services.DataQuerying.Strategies.Queries.OpenZaak.Interfaces;
using EventsHandler.Services.DataQuerying.Strategies.Queries.OpenZaak.v2;
using EventsHandler.Services.Register.Interfaces;
using EventsHandler.Services.Register.v2;
using EventsHandler.Services.Versioning;
using EventsHandler.Services.Versioning.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MoqExt;

namespace EventsHandler.Tests.Unit.Services.Versioning
{
    [TestFixture]
    public sealed class VersionRegisterTests
    {
        [Test]
        public void GetApisVersions_ForNotExistingServices_ReturnsEmptyString()
        {
            // Arrange
            using WebApiConfiguration configuration = ConfigurationHandler.GetWebApiConfiguration();
            IServiceProvider serviceProvider = new Mock<IServiceProvider>().Object;
            IVersionsRegister register = new VersionsRegister(serviceProvider, configuration);

            // Act
            string actualResult = register.GetApisVersions();

            // Assert
            Assert.That(actualResult, Is.Empty);
        }

        [Test]
        public void GetApisVersions_ForExistingServices_ReturnsExpectedString()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            using WebApiConfiguration configuration =
                ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.ValidAppSettings);

            serviceCollection.AddSingleton(configuration);

            serviceCollection.AddSingleton<IQueryZaak, QueryZaak>();
            serviceCollection.AddSingleton<IQueryKlant, QueryKlant>();
            serviceCollection.AddSingleton<IQueryBesluiten, QueryBesluiten>();
            serviceCollection.AddSingleton<IQueryObjecten, QueryObjecten>();
            serviceCollection.AddSingleton<IQueryObjectTypen, QueryObjectTypen>();
            serviceCollection.AddSingleton<ITelemetryService, ContactRegistration>();

            IServiceProvider serviceProvider = new MockingContext(serviceCollection);
            IVersionsRegister register = new VersionsRegister(serviceProvider, configuration);

            // Act
            string actualResult = register.GetApisVersions();

            // Assert
            Assert.That(actualResult, Is.EqualTo("OpenZaak v1.12.1, OpenKlant v2.0.0, Besluiten v1.1.0, Objecten v2.3.1, ObjectTypen v2.2.0, Klantcontacten v2.0.0"));
        }

        [Test]
        public void GetVersions_ForExistingServices_ReturnsExpectedString()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            using WebApiConfiguration configuration =
                ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.BothValid_v1);

            serviceCollection.AddSingleton(configuration);

            serviceCollection.AddSingleton<IQueryZaak, QueryZaak>();
            serviceCollection.AddSingleton<IQueryKlant, QueryKlant>();
            serviceCollection.AddSingleton<IQueryBesluiten, QueryBesluiten>();
            serviceCollection.AddSingleton<IQueryObjecten, QueryObjecten>();
            serviceCollection.AddSingleton<IQueryObjectTypen, QueryObjectTypen>();
            serviceCollection.AddSingleton<ITelemetryService, ContactRegistration>();

            IServiceProvider serviceProvider = new MockingContext(serviceCollection);
            IVersionsRegister register = new VersionsRegister(serviceProvider, configuration);

            const string testVersions = "1, 2, 3";

            // Act
            string actualResult = register.GetOmcVersion(testVersions);

            // Assert
            Assert.That(actualResult, Is.EqualTo($"OMC: v{OmcVersion.GetExpandedVersion()} () | Workflow: v1 ({testVersions})."));
        }
    }
}