// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Mapping.Models.POCOs.OpenZaak.v2;
using EventsHandler.Properties;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Utilities._TestHelpers;

namespace EventsHandler.UnitTests.Mapping.Models.POCOs.OpenZaak.v2
{
    [TestFixture]
    public sealed class CaseRolesTests
    {
        #region Citizen (method)
        [Test]
        public void Citizen_Method_ForMissingResults_ThrowsHttpRequestException()
        {
            // Arrange
            WebApiConfiguration testConfiguration = ConfigurationHandler.GetWebApiConfiguration();

            var caseRoles = new CaseRoles();  // Empty "Results" inside

            // Act & Assert
            AssertThrows<HttpRequestException>(testConfiguration, caseRoles, Resources.HttpRequest_ERROR_EmptyCaseRoles);
        }

        [Test]
        public void Citizen_Method_ForExistingResults_WithoutInitiatorRole_ThrowsHttpRequestException()
        {
            // Arrange
            WebApiConfiguration testConfiguration = ConfigurationHandler.GetValidAppSettingsConfiguration();

            CaseRoles caseRoles = GetTestCaseRoles();  // Invalid "Results" inside

            // Act & Assert
            AssertThrows<HttpRequestException>(testConfiguration, caseRoles, Resources.HttpRequest_ERROR_MissingInitiatorRole);
        }

        [Test]
        public void Citizen_Method_ForExistingResults_WithMultipleInitiatorRoles_ReturnsHttpRequestException()
        {
            // Arrange
            WebApiConfiguration testConfiguration = ConfigurationHandler.GetValidAppSettingsConfiguration();

            string existingInitiatorRole = testConfiguration.AppSettings.Variables.InitiatorRole();
            CaseRoles caseRoles = GetTestCaseRoles(
                new CaseRole { InitiatorRole = existingInitiatorRole },
                new CaseRole { InitiatorRole = existingInitiatorRole });  // Multiple matching results

            // Act & Assert
            AssertThrows<HttpRequestException>(testConfiguration, caseRoles, Resources.HttpRequest_ERROR_MultipleInitiatorRoles);
        }

        [Test]
        public void Citizen_Method_ForExistingResults_WithSingleInitiatorRole_ReturnsCitizenData()
        {
            // Arrange
            WebApiConfiguration testConfiguration = ConfigurationHandler.GetValidAppSettingsConfiguration();

            string existingInitiatorRole = testConfiguration.AppSettings.Variables.InitiatorRole();
            var expectedCitizen = new CitizenData { BsnNumber = "012456789" };
            CaseRoles caseRoles = GetTestCaseRoles(
                new CaseRole { InitiatorRole = existingInitiatorRole, Citizen = expectedCitizen });  // Unique matching result

            // Act
            CitizenData actualCitizen = caseRoles.Citizen(testConfiguration);

            // Assert
            Assert.That(actualCitizen, Is.EqualTo(expectedCitizen));
        }

        #region Helper methods
        private static CaseRoles GetTestCaseRoles(params CaseRole[] roles)
        {
            var caseRoles = new CaseRoles
            {
                Results = new List<CaseRole>
                {
                    new() { InitiatorRole = string.Empty }
                }
            };

            caseRoles.Results.AddRange(roles);

            return caseRoles;
        }

        private static void AssertThrows<TException>(WebApiConfiguration configuration, CaseRoles caseRoles, string exceptionMessage)
            where TException : Exception
        {
            Assert.Multiple(() =>
            {
                TException? exception = Assert.Throws<TException>(() =>
                    caseRoles.Citizen(configuration));

                Assert.That(exception?.Message, Is.EqualTo(exceptionMessage));
            });
        }
        #endregion
        #endregion
    }
}