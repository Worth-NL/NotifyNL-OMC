// © 2026, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using System.Text.Json;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.DataSending.Clients.Factories.Interfaces;
using WebQueries.DataSending.Clients.Interfaces;
using WebQueries.Exceptions;
using WebQueries.MOBB;
using WebQueries.MOBB.Interfaces;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Serialization.Interfaces;

namespace WebQueries.Tests.Unit.MOBB
{
    /// <summary>
    /// The Berichtenbox flow in a deployment that does not use "Open VTB" at all.
    /// </summary>
    /// <remarks>
    ///   Kept apart from <see cref="MessageBoxScenarioImplementationTests"/> because it needs a configuration with
    ///   every environment variable unset, and the configuration caches its values statically.
    /// </remarks>
    [TestFixture]
    public sealed class MessageBoxScenarioNotConfiguredTests
    {
        private OmcConfiguration _configuration = null!;
        private IMessageBoxScenario _scenario = null!;

        [SetUp]
        public void SetupTests()
        {
            this._configuration = ConfigurationHandler.GetOmcConfigurationWith(
                ConfigurationHandler.TestLoaderTypesSetup.InvalidEnvironment_v2);

            // Clears whatever another fixture left in the static configuration cache.
            this._configuration.Dispose();

            // Strict and without setups: the scenario must not touch any of them.
            this._scenario = new MessageBoxScenarioImplementation(
                new Mock<IDataQueryService<NotificationEvent>>(MockBehavior.Strict).Object,
                new Mock<IHttpClientFactory<INotifyClient, string>>(MockBehavior.Strict).Object,
                new Mock<ISerializationService>(MockBehavior.Strict).Object,
                this._configuration,
                new Mock<IServiceProvider>(MockBehavior.Strict).Object,
                NullLogger<MessageBoxScenarioImplementation>.Instance);
        }

        [TearDown]
        public void CleanUpTests()
        {
            this._configuration.Dispose();
        }

        [Test]
        public void ProcessCloudEventAsync_OpenVtbNotConfigured_AbortsWithoutQueryingAnything()
        {
            // NOTE: Leaving ZGW_ENDPOINT_OPENVTB unset is how a deployment opts out of this flow. A CloudEvent
            //       that arrives anyway is a decision to drop (206), not a failure to retry - and it must not
            //       reach for Open VTB, OpenKlant or Notify NL, which the strict mocks enforce.

            // Arrange
            JsonElement cloudEvent = JsonSerializer.SerializeToElement(new
            {
                type = "nl.overheid.berichten.bericht-gepubliceerd",
                subject = Guid.NewGuid().ToString()
            });

            // Act & Assert
            ProcessingAbortedException? exception = Assert.ThrowsAsync<ProcessingAbortedException>(
                async () => await this._scenario.ProcessCloudEventAsync(cloudEvent));

            Assert.That(exception!.Message, Does.Contain("ZGW_ENDPOINT_OPENVTB"));
        }
    }
}
