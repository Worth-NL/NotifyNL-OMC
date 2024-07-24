// © 2024, Worth Systems.

using EventsHandler.Services.DataQuerying.Composition.Strategy.Objecten.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.Objecten.v1;
using EventsHandler.Services.DataQuerying.Composition.Strategy.ObjectTypen.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.ObjectTypen.v1;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.v2;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.v2;
using EventsHandler.Services.Telemetry.Interfaces;
using EventsHandler.Services.Telemetry.v2;
using EventsHandler.Services.Versioning;
using EventsHandler.Services.Versioning.Interfaces;
using EventsHandler.Utilities._TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MoqExt;

namespace EventsHandler.UnitTests.Behaviors.Versioning
{
    [TestFixture]
    public sealed class VersionRegisterTests
    {
        [Test]
        public void GetVersions_ForNotExistingServices_ReturnsEmptyString()
        {
            // Arrange
            IServiceProvider serviceProvider = new Mock<IServiceProvider>().Object;
            IVersionsRegister register = new VersionsRegister(serviceProvider);
            
            // Act
            string actualResult = register.GetVersions();

            // Assert
            Assert.That(actualResult, Is.Empty);
        }

        [Test]
        public void GetVersions_ForExistingServices_ReturnsExpectedString()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(ConfigurationHandler.GetValidAppSettingsConfiguration());
            serviceCollection.AddSingleton<IQueryZaak, QueryZaak>();
            serviceCollection.AddSingleton<IQueryKlant, QueryKlant>();
            serviceCollection.AddSingleton<IQueryObjecten, QueryObjecten>();
            serviceCollection.AddSingleton<IQueryObjectTypen, QueryObjectTypen>();
            serviceCollection.AddSingleton<ITelemetryService, ContactRegistration>();

            IServiceProvider serviceProvider = new MockingContext(serviceCollection);
            IVersionsRegister register = new VersionsRegister(serviceProvider);
            
            // Act
            string actualResult = register.GetVersions();

            // Assert
            Assert.That(actualResult, Is.EqualTo("(OpenZaak v1.12.1, OpenKlant v2.0.0, Objecten v2.3.1, ObjectTypen v2.2.0, Klantcontacten v2.0.0)"));
        }
    }
}