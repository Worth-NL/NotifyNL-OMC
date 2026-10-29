// © 2026, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using ZgwModels.Mapping.Events;
using ZgwModels.Serialization;

namespace ZgwModels.Tests.Unit.Mapping.Events
{
    /// <summary>
    /// A rejected payload's reason ends up in the response and in Sentry, so it names what was received, never its
    /// values: a CloudEvent's "data" can hold a citizen's BSN (#267).
    /// </summary>
    [TestFixture]
    public sealed class CloudEventNormalizerTests
    {
        private OmcConfiguration _configuration = null!;
        private CloudEventNormalizer _normalizer = null!;

        [SetUp]
        public void SetupTests()
        {
            this._configuration = ConfigurationHandler.GetOmcConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.ValidEnvironment_v2);
            this._normalizer = new CloudEventNormalizer(this._configuration, NullLogger<CloudEventNormalizer>.Instance, new SpecificSerializer());
        }

        [TearDown]
        public void CleanUpTests()
        {
            this._configuration.Dispose();
        }

        [Test]
        public void Normalize_CloudEventMissingAnAttribute_ReasonNamesTheAttributes_NotTheirValues()
        {
            // Arrange: no "time"
            JObject payload = JObject.FromObject(new
            {
                specversion = "1.0",
                id = "66666666-6666-6666-6666-666666666666",
                source = "urn:nld:oin:01823288444:zaaksysteem",
                type = "nl.overheid.zaken.zaak-gemuteerd",
                subject = "55555555-5555-5555-5555-555555555555",
                data = new { ontvanger = "urn:nld:bsn:nummer:123456789" }
            });

            // Act
            CloudEvent? result = this._normalizer.Normalize(payload, out string? reason);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Null);
                Assert.That(reason, Does.Contain("missing 'time'"));
                Assert.That(reason, Does.Contain("specversion, id, source, type, subject, data"));
                Assert.That(reason, Does.Not.Contain("urn:nld:bsn"));
                Assert.That(reason, Does.Not.Contain("zaaksysteem"));
            });
        }

        [Test]
        public void Normalize_NotificationInAnUnsupportedFormat_ReasonNamesTheAttributes_NotTheirValues()
        {
            // Arrange: an Open Notificaties notification that maps to no CloudEvent type
            JObject payload = JObject.FromObject(new
            {
                actie = "create",
                kanaal = "objecten",
                resource = "object",
                kenmerken = new { bsn = "123456789" },
                hoofdObject = "https://objecten.test/api/v2/objects/55555555-5555-5555-5555-555555555555",
                resourceUrl = "https://objecten.test/api/v2/objects/55555555-5555-5555-5555-555555555555",
                aanmaakdatum = "2026-01-01T00:00:00Z"
            });

            // Act
            CloudEvent? result = this._normalizer.Normalize(payload, out string? reason);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Null);
                Assert.That(reason, Does.Contain("kenmerken"));
                Assert.That(reason, Does.Not.Contain("123456789"));
                Assert.That(reason, Does.Not.Contain("objecten.test"));
            });
        }
    }
}
