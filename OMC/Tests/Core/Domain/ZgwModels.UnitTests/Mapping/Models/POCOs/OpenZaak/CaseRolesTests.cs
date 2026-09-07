// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using NUnit.Framework;
using ZgwModels.Mapping.Models.POCOs.OpenZaak;
using ZgwModels.Properties;

namespace ZgwModels.Tests.Unit.Mapping.Models.POCOs.OpenZaak
{
    [TestFixture]
    public sealed class CaseRolesTests
    {
        private OmcConfiguration _testConfiguration = null!;

        [OneTimeSetUp]
        public void TestsInitialize()
        {
            this._testConfiguration = ConfigurationHandler.GetOmcConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.ValidAppSettings);
        }

        [OneTimeTearDown]
        public void TestsCleanup()
        {
            this._testConfiguration.Dispose();
        }

        #region CaseRole (method)
        [Test]
        public void CaseRole_Method_ForMissingResults_ThrowsHttpRequestException()
        {
            OmcConfiguration emptyConfig = ConfigurationHandler.GetOmcConfiguration();
            var caseRoles = new CaseRoles();

            HttpRequestException? exception = Assert.Throws<HttpRequestException>(() =>
                caseRoles.CaseRole(emptyConfig));

            Assert.That(exception?.Message, Is.EqualTo(ZgwResources.HttpRequest_ERROR_EmptyCaseRoles));

            emptyConfig.Dispose();
        }

        [Test]
        public void CaseRole_Method_ForExistingResults_WithoutInitiatorRole_ThrowsHttpRequestException()
        {
            CaseRoles caseRoles = GetTestCaseRoles();

            HttpRequestException? exception = Assert.Throws<HttpRequestException>(() =>
                caseRoles.CaseRole(this._testConfiguration));

            Assert.That(exception?.Message, Is.EqualTo(ZgwResources.HttpRequest_ERROR_MissingInitiatorRole));
        }

        [Test]
        public void CaseRole_Method_ForExistingResults_WithMatchingInitiatorRole_ReturnsCaseRole()
        {
            string initiatorRole = this._testConfiguration.AppSettings.Variables.InitiatorRole();
            var expectedParty = new PartyData { BsnNumber = "123456789" };
            CaseRoles caseRoles = GetTestCaseRoles(
                new CaseRole { InitiatorRole = initiatorRole, Party = expectedParty });

            PartyData actualParty = caseRoles.CaseRole(this._testConfiguration).Party!.Value;

            Assert.That(actualParty, Is.EqualTo(expectedParty));
        }
        #endregion

        #region Helper methods
        private static CaseRoles GetTestCaseRoles(params CaseRole[] roles)
        {
            var caseRoles = new CaseRoles
            {
                Results =
                [
                    new CaseRole { InitiatorRole = string.Empty }
                ]
            };

            caseRoles.Results.AddRange(roles);

            return caseRoles;
        }
        #endregion
    }
}
