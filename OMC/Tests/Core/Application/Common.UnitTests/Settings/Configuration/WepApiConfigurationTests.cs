// © 2023, Worth Systems.

using Common.Properties;
using Common.Settings.Configuration;
using Common.Settings.Enums;
using Common.Settings.Strategy.Interfaces;
using Common.Tests.Utilities._TestHelpers;
using NUnit.Framework;

// ReSharper disable SuggestVarOrType_SimpleTypes

namespace Common.Tests.Unit.Settings.Configuration
{
    [TestFixture]
    public sealed class WepApiConfigurationTests
    {
        private static OmcConfiguration? s_testConfiguration;

        [TearDown]
        public void TestCleanup()
        {
            s_testConfiguration?.Dispose();
        }

        #region AppSettings
        [Test]
        public void WebApiConfiguration_Valid_AppSettings_ReturnsNotDefaultValues()  // NOTE: This test is checking two things: 1. Loading appsettings.json (from the test file) as expected
        {                                                                            //                                         2. Using second LoadingContext by FallbackContextWrapper logic
            // Arrange
            s_testConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.ValidAppSettings);

            // Act & Assert
            Assert.Multiple(() =>
            {
                // NOTE: Exception would occur if the configurations are invalid
                string result = OmcConfiguration.TestAppSettingsConfigs(s_testConfiguration);
                
                TestContext.Out.WriteLine(result);
            });
        }

        [Test]
        public void WebApiConfiguration_FallbackStrategy_EnvVars_OverloadsAppSettings()
        {
            // Arrange
            s_testConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.EnvVar_Overloading_AppSettings);

            // Act & Assert
            Assert.Multiple(() =>
            {
                Assert.That(s_testConfiguration.AppSettings.Variables.PartyIdentifier(), Does.StartWith(ConfigurationHandler.EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.EmailGenericDescription(), Does.StartWith(ConfigurationHandler.EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.PhoneGenericDescription(), Does.StartWith(ConfigurationHandler.EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.OpenKlant.CodeObjectType(), Does.StartWith(ConfigurationHandler.EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.UxMessages.SMS_Success_Subject(), Does.StartWith(ConfigurationHandler.EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.UxMessages.Email_Failure_Body(), Does.StartWith(ConfigurationHandler.EnvPrefix));
            });
        }

        [Test]
        public void WebApiConfiguration_FallbackStrategy_IfEnvVariableNotPresent_KeepsAppSettings()
        {
            // Arrange
            s_testConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.ValidAppSettings);

            // Act & Assert
            Assert.Multiple(() =>
            {
                Assert.That(s_testConfiguration.AppSettings.Variables.PartyIdentifier(), Does.Not.StartWith(ConfigurationHandler.EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.EmailGenericDescription(), Does.Not.StartWith(ConfigurationHandler.EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.PhoneGenericDescription(), Does.Not.StartWith(ConfigurationHandler.EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.OpenKlant.CodeObjectType(), Does.Not.StartWith(ConfigurationHandler.EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.UxMessages.SMS_Success_Subject(), Does.Not.StartWith(ConfigurationHandler.EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.UxMessages.Email_Failure_Body(), Does.Not.StartWith(ConfigurationHandler.EnvPrefix));
            });
        }
        #endregion

        #region Environment variables
        [Test]
        public void WebApiConfiguration_Valid_EnvironmentVariables_ReturnsNotDefaultValues()
        {
            // Arrange
            s_testConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.ValidEnvironment_v1);

            // Act & Assert
            Assert.Multiple(() =>
            {
                // NOTE: Exception would occur if the configurations are invalid
                string result = OmcConfiguration.TestEnvVariablesConfigs(s_testConfiguration);
                
                TestContext.Out.WriteLine(result);
            });
        }

        [TestCaseSource(nameof(GetTestCases))]
        public void WebApiConfiguration_InEnvironmentMode_ForSelectedInvalidVariables_ThrowsExceptions(
            (string CaseId, TestDelegate Logic, string ExpectedErrorMessage) test)
        {
            // Arrange
            s_testConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.InvalidEnvironment_v1);

            // Act & Assert
            Assert.Multiple(() =>
            {
                ArgumentException? exception = Assert.Throws<ArgumentException>(test.Logic);

                string expectedFullMessage = test.ExpectedErrorMessage;
                int leftCurlyBracketIndex = expectedFullMessage.IndexOf('{', StringComparison.Ordinal);
                string expectedTrimmedMessage = expectedFullMessage[..leftCurlyBracketIndex];

                Assert.That(exception?.Message.StartsWith(expectedTrimmedMessage), Is.True,
                    message: $"{test.CaseId}: {exception?.Message}");
            });
        }

        private static IEnumerable<(string CaseId, TestDelegate ActualMethod, string ExpectedErrorMessage)> GetTestCases()
        {
            // Invalid: ""
            yield return ("#1", () => s_testConfiguration!.ZGW.Endpoint.OpenNotificaties(), CommonResources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: " "
            yield return ("#2", () => s_testConfiguration!.ZGW.Endpoint.OpenZaak(), CommonResources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: http://domain
            yield return ("#3", () => s_testConfiguration!.ZGW.Endpoint.OpenKlant(), CommonResources.Configuration_ERROR_ContainsHttp);
            // Invalid: https://domain
            yield return ("#4", () => s_testConfiguration!.ZGW.Endpoint.Objecten(), CommonResources.Configuration_ERROR_ContainsHttp);
            // Invalid: Default URI
            yield return ("#5", () => s_testConfiguration!.Notify.API.BaseUrl(), CommonResources.Configuration_ERROR_InvalidUri);
            // Invalid: Not existing
            yield return ("#6", () => s_testConfiguration!.Notify.API.Key(), CommonResources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: Empty
            yield return ("#7", () => s_testConfiguration!.Notify.TemplateId.Sms.ZaakCreate(), CommonResources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: Empty
            yield return ("#8", () => s_testConfiguration!.Notify.TemplateId.Sms.ZaakUpdate(), CommonResources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: 8-4-(2-2)-4-12
            yield return ("#9", () => s_testConfiguration!.Notify.TemplateId.Sms.ZaakClose(), CommonResources.Configuration_ERROR_InvalidTemplateId);
            // Invalid: (9)-4-4-4-12
            yield return ("#10", () => s_testConfiguration!.Notify.TemplateId.Sms.TaskAssigned(), CommonResources.Configuration_ERROR_InvalidTemplateId);
            // Invalid: Special characters
            yield return ("#11", () => s_testConfiguration!.Notify.TemplateId.Sms.MessageReceived(), CommonResources.Configuration_ERROR_InvalidTemplateId);
        }

        [TestCase("1", true)]
        [TestCase("9", false)]
        [TestCase("", false)]
        [TestCase(" ", false)]
        public void IsAllowed_InEnvironmentMode_ForSpecificCaseId_ReturnsExpectedResult(string caseId, bool expectedResult)
        {
            // Arrange
            s_testConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.ValidEnvironment_v1);

            // Act
            #pragma warning disable IDE0008  // Using "explicit types" wouldn't help with readability of the code
            var whitelistedIDs = s_testConfiguration.ZGW.Whitelist.ZaakCreate_IDs();
            #pragma warning restore IDE00008
            bool isAllowed = whitelistedIDs.IsAllowed(caseId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(whitelistedIDs.Count, Is.EqualTo(3));
                Assert.That(isAllowed, Is.EqualTo(expectedResult));
            });
        }

        [Test]
        public void IsAllowed_InEnvironmentMode_ForEmptyWhitelistedIDs_ReturnsFalse()
        {
            // Arrange
            s_testConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.InvalidEnvironment_v1);

            // Act
            #pragma warning disable IDE0008  // Using "explicit types" wouldn't help with readability of the code
            var whitelistedIDs = s_testConfiguration.ZGW.Whitelist.ZaakCreate_IDs();
            #pragma warning restore IDE00008
            bool isAllowed = whitelistedIDs.IsAllowed("1");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(whitelistedIDs.Count, Is.Zero);
                Assert.That(isAllowed, Is.False);
            });
        }

        [Test]
        public void OpenKlant_InEnvironmentMode_OmcWorkflowV1_ApiKeyIsNotRequired()
        {
            // Arrange
            using OmcConfiguration configuration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.InvalidEnvironment_v1);

            // Act
            string openKlantApiKey = configuration.ZGW.Auth.Key.OpenKlant();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(configuration.OMC.Feature.Workflow_Version(), Is.EqualTo(1));
                Assert.That(openKlantApiKey, Is.Empty);
            });
        }

        [Test]
        public void OpenKlant_InEnvironmentMode_OmcWorkflowV2_ApiKeyIsRequired()
        {
            // Arrange
            using OmcConfiguration configuration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.InvalidEnvironment_v2);

            // Act & Assert
            Assert.Multiple(() =>
            {
                Assert.That(configuration.OMC.Feature.Workflow_Version(), Is.EqualTo(2));
                ArgumentException? exception = Assert.Throws<ArgumentException>(() => configuration.ZGW.Auth.Key.OpenKlant());
                Assert.That(exception?.Message, Is.EqualTo(CommonResources.Configuration_ERROR_ValueNotFoundOrEmpty
                    .Replace("{0}", "ZGW_AUTH_KEY_OPENKLANT")));
            });
        }
        #endregion

        #region FallbackContextWrapper
        private const string TestParentNode = "Parent";
        private const string TestChildNode = "Child";

        [Test]
        public void Constructor_InitializesPropertiesAsExpected()
        {
            // Arrange
            ILoadersContext appSettingsContext = ConfigurationHandler.GetLoadersContext(LoaderTypes.AppSettings);
            ILoadersContext environmentContext = ConfigurationHandler.GetLoadersContext(LoaderTypes.Environment);

            // Act
            OmcConfiguration.FallbackContextWrapper contextWrapper = new(appSettingsContext, environmentContext, TestParentNode, TestChildNode);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(contextWrapper.PrimaryLoaderContext, Is.EqualTo(appSettingsContext));
                Assert.That(contextWrapper.FallbackLoaderContext, Is.EqualTo(environmentContext));

                Assert.That(contextWrapper.PrimaryCurrentPath, Is.EqualTo($"{TestParentNode}:{TestChildNode}"));
                Assert.That(contextWrapper.FallbackCurrentPath, Is.EqualTo($"{TestParentNode.ToUpper()}_{TestChildNode.ToUpper()}"));
            });
        }

        [Test]
        public void Update_InitializesPropertiesAsExpected()
        {
            // Arrange
            ILoadersContext appSettingsContext = ConfigurationHandler.GetLoadersContext(LoaderTypes.AppSettings);
            ILoadersContext environmentContext = ConfigurationHandler.GetLoadersContext(LoaderTypes.Environment);

            const string testExtensionNode = "Extra";

            OmcConfiguration.FallbackContextWrapper contextWrapper = new(appSettingsContext, environmentContext, TestParentNode, TestChildNode);
            
            // Act
            contextWrapper = contextWrapper.Update(testExtensionNode);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(contextWrapper.PrimaryLoaderContext, Is.EqualTo(appSettingsContext));
                Assert.That(contextWrapper.FallbackLoaderContext, Is.EqualTo(environmentContext));

                Assert.That(contextWrapper.PrimaryCurrentPath, Is.EqualTo($"{TestParentNode}:{TestChildNode}:{testExtensionNode}"));
                Assert.That(contextWrapper.FallbackCurrentPath, Is.EqualTo($"{TestParentNode.ToUpper()}_{TestChildNode.ToUpper()}_{testExtensionNode.ToUpper()}"));
            });
        }
        #endregion
    }
}