﻿// © 2024, Worth Systems.

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
        private WebApiConfiguration _testConfiguration = null!;

        [OneTimeSetUp]
        public void TestsInitialize()
        {
            this._testConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypes.ValidAppSettings);
        }

        [OneTimeTearDown]
        public void TestsCleanup()
        {
            this._testConfiguration.Dispose();
        }

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
            CaseRoles caseRoles = GetTestCaseRoles();  // Invalid "Results" inside

            // Act & Assert
            AssertThrows<HttpRequestException>(this._testConfiguration, caseRoles, Resources.HttpRequest_ERROR_MissingInitiatorRole);
        }

        [Test]
        public void Citizen_Method_ForExistingResults_WithSingleInitiatorRole_ReturnsCitizenData()
        {
            // Arrange
            string existingInitiatorRole = this._testConfiguration.AppSettings.Variables.InitiatorRole();
            var expectedCitizen = new PartyData { BsnNumber = "012456789" };
            CaseRoles caseRoles = GetTestCaseRoles(
                new CaseRole { InitiatorRole = existingInitiatorRole, Party = expectedCitizen });  // Unique matching result

            // Act
            PartyData actualParty = caseRoles.CaseRole(this._testConfiguration).Party;

            // Assert
            Assert.That(actualParty, Is.EqualTo(expectedCitizen));
        }
        #endregion

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
                    caseRoles.CaseRole(configuration));

                Assert.That(exception?.Message, Is.EqualTo(exceptionMessage));
            });
        }
        #endregion
    }
}