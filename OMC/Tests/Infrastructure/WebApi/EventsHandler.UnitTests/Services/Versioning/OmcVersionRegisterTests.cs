// © 2024, Worth Systems.

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
            IVersionRegister register = new OmcVersionRegister();

            const string testVersions = "1, 2, 3";

            // Act
            string actualResult = register.GetVersion(testVersions);

            // Assert
            Assert.That(actualResult, Is.EqualTo($"OMC: v{OmcVersion.GetExpandedVersion()} () | {testVersions}."));
        }
    }
}
