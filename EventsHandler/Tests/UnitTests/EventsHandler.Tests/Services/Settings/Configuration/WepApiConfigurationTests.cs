// © 2023, Worth Systems.

using EventsHandler.Properties;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Settings.Enums;
using EventsHandler.Services.Settings.Strategy.Interfaces;
using static EventsHandler.Utilities._TestHelpers.ConfigurationHandler;

// ReSharper disable SuggestVarOrType_SimpleTypes

namespace EventsHandler.UnitTests.Services.Settings.Configuration
{
    [TestFixture]
    public sealed class WepApiConfigurationTests
    {
        private static WebApiConfiguration? s_testConfiguration;

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
            s_testConfiguration = GetWebApiConfigurationWith(TestLoaderTypesSetup.ValidAppSettings);

            // Act & Assert
            Assert.Multiple(() =>
            {
                // NOTE: Exception would occur if the configurations are invalid
                string result = WebApiConfiguration.TestAppSettingsConfigs(s_testConfiguration);
                
                TestContext.Out.WriteLine(result);
            });
        }

        [Test]
        public void WebApiConfiguration_FallbackStrategy_EnvVars_OverloadsAppSettings()
        {
            // Arrange
            s_testConfiguration = GetWebApiConfigurationWith(TestLoaderTypesSetup.EnvVar_Overloading_AppSettings);

            // Act & Assert
            Assert.Multiple(() =>
            {
                Assert.That(s_testConfiguration.AppSettings.Variables.PartyIdentifier(), Does.StartWith(EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.EmailGenericDescription(), Does.StartWith(EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.PhoneGenericDescription(), Does.StartWith(EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.OpenKlant.CodeObjectType(), Does.StartWith(EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.UxMessages.SMS_Success_Subject(), Does.StartWith(EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.UxMessages.Email_Failure_Body(), Does.StartWith(EnvPrefix));
            });
        }

        [Test]
        public void WebApiConfiguration_FallbackStrategy_IfEnvVariableNotPresent_KeepsAppSettings()
        {
            // Arrange
            s_testConfiguration = GetWebApiConfigurationWith(TestLoaderTypesSetup.ValidAppSettings);

            // Act & Assert
            Assert.Multiple(() =>
            {
                Assert.That(s_testConfiguration.AppSettings.Variables.PartyIdentifier(), Does.Not.StartWith(EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.EmailGenericDescription(), Does.Not.StartWith(EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.PhoneGenericDescription(), Does.Not.StartWith(EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.OpenKlant.CodeObjectType(), Does.Not.StartWith(EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.UxMessages.SMS_Success_Subject(), Does.Not.StartWith(EnvPrefix));
                Assert.That(s_testConfiguration.AppSettings.Variables.UxMessages.Email_Failure_Body(), Does.Not.StartWith(EnvPrefix));
            });
        }
        #endregion

        #region Environment variables
        [Test]
        public void WebApiConfiguration_Valid_EnvironmentVariables_ReturnsNotDefaultValues()
        {
            // Arrange
            s_testConfiguration = GetWebApiConfigurationWith(TestLoaderTypesSetup.ValidEnvironment_v1);

            // Act & Assert
            Assert.Multiple(() =>
            {
                // NOTE: Exception would occur if the configurations are invalid
                string result = WebApiConfiguration.TestEnvVariablesConfigs(s_testConfiguration);
                
                TestContext.Out.WriteLine(result);
            });
        }

        [TestCaseSource(nameof(GetTestCases))]
        public void WebApiConfiguration_InEnvironmentMode_ForSelectedInvalidVariables_ThrowsExceptions(
            (string CaseId, TestDelegate Logic, string ExpectedErrorMessage) test)
        {
            // Arrange
            s_testConfiguration = GetWebApiConfigurationWith(TestLoaderTypesSetup.InvalidEnvironment_v1);

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
            yield return ("#1", () => s_testConfiguration!.ZGW.Endpoint.OpenNotificaties(), Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: " "
            yield return ("#2", () => s_testConfiguration!.ZGW.Endpoint.OpenZaak(), Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: http://domain
            yield return ("#3", () => s_testConfiguration!.ZGW.Endpoint.OpenKlant(), Resources.Configuration_ERROR_ContainsHttp);
            // Invalid: https://domain
            yield return ("#4", () => s_testConfiguration!.ZGW.Endpoint.Objecten(), Resources.Configuration_ERROR_ContainsHttp);
            // Invalid: Default URI
            yield return ("#5", () => s_testConfiguration!.Notify.API.BaseUrl(), Resources.Configuration_ERROR_InvalidUri);
            // Invalid: Not existing
            yield return ("#6", () => s_testConfiguration!.Notify.API.Key(), Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: Empty
            yield return ("#7", () => s_testConfiguration!.Notify.TemplateId.Sms.ZaakCreate(), Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: Empty
            yield return ("#8", () => s_testConfiguration!.Notify.TemplateId.Sms.ZaakUpdate(), Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: 8-4-(2-2)-4-12
            yield return ("#9", () => s_testConfiguration!.Notify.TemplateId.Sms.ZaakClose(), Resources.Configuration_ERROR_InvalidTemplateId);
            // Invalid: (9)-4-4-4-12
            yield return ("#10", () => s_testConfiguration!.Notify.TemplateId.Sms.TaskAssigned(), Resources.Configuration_ERROR_InvalidTemplateId);
            // Invalid: Special characters
            yield return ("#11", () => s_testConfiguration!.Notify.TemplateId.Sms.MessageReceived(), Resources.Configuration_ERROR_InvalidTemplateId);
        }

        [TestCase("1", true)]
        [TestCase("9", false)]
        [TestCase("", false)]
        [TestCase(" ", false)]
        public void IsAllowed_InEnvironmentMode_ForSpecificCaseId_ReturnsExpectedResult(string caseId, bool expectedResult)
        {
            // Arrange
            s_testConfiguration = GetWebApiConfigurationWith(TestLoaderTypesSetup.ValidEnvironment_v1);

            // Act
            var whitelistedIDs = s_testConfiguration.ZGW.Whitelist.ZaakCreate_IDs();
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
            s_testConfiguration = GetWebApiConfigurationWith(TestLoaderTypesSetup.InvalidEnvironment_v1);

            // Act
            var whitelistedIDs = s_testConfiguration.ZGW.Whitelist.ZaakCreate_IDs();
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
            using var configuration = GetWebApiConfigurationWith(TestLoaderTypesSetup.InvalidEnvironment_v1);

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
            using var configuration = GetWebApiConfigurationWith(TestLoaderTypesSetup.InvalidEnvironment_v2);

            // Act & Assert
            Assert.Multiple(() =>
            {
                Assert.That(configuration.OMC.Feature.Workflow_Version(), Is.EqualTo(2));
                ArgumentException? exception = Assert.Throws<ArgumentException>(() => configuration.ZGW.Auth.Key.OpenKlant());
                Assert.That(exception?.Message, Is.EqualTo(Resources.Configuration_ERROR_ValueNotFoundOrEmpty
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
            ILoadersContext appSettingsContext = GetLoadersContext(LoaderTypes.AppSettings);
            ILoadersContext environmentContext = GetLoadersContext(LoaderTypes.Environment);

            // Act
            WebApiConfiguration.FallbackContextWrapper contextWrapper = new(appSettingsContext, environmentContext, TestParentNode, TestChildNode);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(contextWrapper.PrimaryLoadersContext, Is.EqualTo(appSettingsContext));
                Assert.That(contextWrapper.FallbackLoadersContext, Is.EqualTo(environmentContext));

                Assert.That(contextWrapper.PrimaryCurrentPath, Is.EqualTo($"{TestParentNode}:{TestChildNode}"));
                Assert.That(contextWrapper.FallbackCurrentPath, Is.EqualTo($"{TestParentNode.ToUpper()}_{TestChildNode.ToUpper()}"));
            });
        }

        [Test]
        public void Update_InitializesPropertiesAsExpected()
        {
            // Arrange
            ILoadersContext appSettingsContext = GetLoadersContext(LoaderTypes.AppSettings);
            ILoadersContext environmentContext = GetLoadersContext(LoaderTypes.Environment);

            const string testExtensionNode = "Extra";

            WebApiConfiguration.FallbackContextWrapper contextWrapper = new(appSettingsContext, environmentContext, TestParentNode, TestChildNode);
            
            // Act
            contextWrapper = contextWrapper.Update(testExtensionNode);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(contextWrapper.PrimaryLoadersContext, Is.EqualTo(appSettingsContext));
                Assert.That(contextWrapper.FallbackLoadersContext, Is.EqualTo(environmentContext));

                Assert.That(contextWrapper.PrimaryCurrentPath, Is.EqualTo($"{TestParentNode}:{TestChildNode}:{testExtensionNode}"));
                Assert.That(contextWrapper.FallbackCurrentPath, Is.EqualTo($"{TestParentNode.ToUpper()}_{TestChildNode.ToUpper()}_{testExtensionNode.ToUpper()}"));
            });
        }
        #endregion
    }
}