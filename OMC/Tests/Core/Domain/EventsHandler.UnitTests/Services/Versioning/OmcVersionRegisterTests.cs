// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using Common.Versioning.Interfaces;
using Common.Versioning.Models;
using EventsHandler.Versioning;

namespace EventsHandler.Tests.Unit.Services.Versioning
{
    internal class OmcVersionRegisterTests
    {
        [Test]
        public void GetVersion_ForExistingServices_ReturnsExpectedString()
        {
            // Arrange
            using OmcConfiguration configuration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.ValidEnvironment_v1);

            IVersionRegister register = new OmcVersionRegister(configuration);

            const string testVersions = "1, 2, 3";

            // Act
            string actualResult = register.GetVersion(testVersions);

            // Assert
            Assert.That(actualResult, Is.EqualTo($"OMC: v{OmcVersion.GetExpandedVersion()} () | Workflow: v1 ({testVersions})."));
        }
    }
}