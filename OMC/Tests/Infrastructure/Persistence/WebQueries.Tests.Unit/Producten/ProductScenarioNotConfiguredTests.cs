// © 2026, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Clients.Factories.Interfaces;
using WebQueries.DataSending.Clients.Interfaces;
using WebQueries.Exceptions;
using WebQueries.Producten;
using WebQueries.Producten.Interfaces;
using WebQueries.Register.Interfaces;
using ZgwModels.Mapping.Enums.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Serialization.Interfaces;

namespace WebQueries.Tests.Unit.Producten
{
    /// <summary>
    /// The "Product created" scenario in a deployment that does not use "Open Product" at all.
    /// </summary>
    /// <remarks>
    ///   Kept apart from <see cref="ProductScenarioImplementationTests"/> because it needs a configuration with
    ///   every environment variable unset, and the configuration caches its values statically.
    /// </remarks>
    [TestFixture]
    public sealed class ProductScenarioNotConfiguredTests
    {
        private OmcConfiguration _configuration = null!;
        private IProductScenario _scenario = null!;

        [SetUp]
        public void SetupTests()
        {
            this._configuration = ConfigurationHandler.GetOmcConfigurationWith(
                ConfigurationHandler.TestLoaderTypesSetup.InvalidEnvironment_v2);

            // Clears whatever another fixture left in the static configuration cache.
            this._configuration.Dispose();

            // Strict and without setups: the scenario must not touch any of them.
            this._scenario = new ProductScenarioImplementation(
                new Mock<IDataQueryService<NotificationEvent>>(MockBehavior.Strict).Object,
                new Mock<IHttpClientFactory<INotifyClient, string>>(MockBehavior.Strict).Object,
                new Mock<ISerializationService>(MockBehavior.Strict).Object,
                new Mock<ITelemetryService>(MockBehavior.Strict).Object,
                this._configuration,
                NullLogger<ProductScenarioImplementation>.Instance);
        }

        [TearDown]
        public void CleanUpTests()
        {
            this._configuration.Dispose();
        }

        [Test]
        public void ProcessProductAsync_OpenProductNotConfigured_AbortsWithoutQueryingAnything()
        {
            // NOTE: Leaving ZGW_ENDPOINT_OPENPRODUCTEN unset is how a deployment opts out of this scenario.
            //       An event that arrives anyway is a decision to drop (206), not a failure to retry - and it
            //       must not reach for Open Product, OpenKlant or Notify NL, which the strict mocks enforce.

            // Arrange
            Uri productUri = new($"https://openproduct.test/producten/api/v1/producten/{Guid.NewGuid()}");

            NotificationEvent notification = new()
            {
                Action = Actions.Create,
                Channel = Channels.Products,
                Resource = Resources.Product,
                MainObjectUri = productUri,
                ResourceUri = productUri
            };

            // Act & Assert
            ProcessingAbortedException? exception = Assert.ThrowsAsync<ProcessingAbortedException>(
                async () => await this._scenario.ProcessProductAsync(notification));

            Assert.That(exception!.Message, Does.Contain("ZGW_ENDPOINT_OPENPRODUCTEN"));
        }
    }
}
