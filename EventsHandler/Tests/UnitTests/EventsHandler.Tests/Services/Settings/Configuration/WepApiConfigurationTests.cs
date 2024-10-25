// © 2023, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Properties;
using EventsHandler.Services.Settings.Attributes;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Utilities._TestHelpers;
using System.Reflection;
using static EventsHandler.Utilities._TestHelpers.ConfigurationHandler;

#pragma warning disable IDE0008  // Declaration of static types would be too long
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
        public void WebApiConfiguration_Valid_AppSettings_ReturnsNotDefaultValues()
        {
            // Arrange
            s_testConfiguration = GetWebApiConfigurationWith(TestLoaderTypes.ValidAppSettings);

            int counter = 0;
            List<string> methodNames = new();

            // Act & Assert
            Assert.Multiple(() =>
            {
                var appSettings = s_testConfiguration.AppSettings;

                // AppSettings | Network
                TestConfigProperties(ref counter, methodNames, appSettings.Network);

                // AppSettings | Encryption
                TestConfigProperties(ref counter, methodNames, appSettings.Encryption);

                var variablesSettings = s_testConfiguration.AppSettings.Variables;

                // AppSettings | Variables
                TestConfigProperties(ref counter, methodNames, variablesSettings, isRecursionEnabled: false);

                // AppSettings | Variables | OpenKlant
                TestConfigProperties(ref counter, methodNames, variablesSettings.OpenKlant);

                // AppSettings | Variables | UX Messages
                TestConfigProperties(ref counter, methodNames, variablesSettings.UxMessages);

                TestContext.WriteLine($"Tested environment variables: {counter}{Environment.NewLine}");
                TestContext.WriteLine($"Methods: {methodNames.Join()}");
            });
        }
        #endregion

        #region Environment variables
        [Test]
        public void WebApiConfiguration_Valid_EnvironmentVariables_ReturnsNotDefaultValues()
        {
            // Arrange
            s_testConfiguration = GetWebApiConfigurationWith(TestLoaderTypes.ValidEnvironment_v1);

            int counter = 0;
            List<string> methodNames = new();

            // Act & Assert
            Assert.Multiple(() =>
            {
                var omcConfiguration = s_testConfiguration.OMC;

                // OMC | Authorization | JWT
                TestConfigProperties(ref counter, methodNames, omcConfiguration.Auth.JWT);

                // OMC | Features
                TestConfigProperties(ref counter, methodNames, omcConfiguration.Features);
                
                var zgwConfiguration = s_testConfiguration.ZGW;

                // ZGW | Authorization | JWT
                TestConfigProperties(ref counter, methodNames, zgwConfiguration.Auth.JWT);

                // ZGW | API | Key
                TestConfigProperties(ref counter, methodNames, zgwConfiguration.API.Key);

                // ZGW | Domain
                TestConfigProperties(ref counter, methodNames, zgwConfiguration.Domain);

                // ZGW | Templates (Email + SMS)
                TestConfigProperties(ref counter, methodNames, zgwConfiguration.TemplateIds.Email);
                TestConfigProperties(ref counter, methodNames, zgwConfiguration.TemplateIds.Sms);

                // ZGW | Whitelist
                TestConfigProperties(ref counter, methodNames, zgwConfiguration.Whitelist);

                // ZGW | Variables | Objecten
                TestConfigProperties(ref counter, methodNames, zgwConfiguration.Variables.Objecten);
                
                var notifyConfiguration = s_testConfiguration.Notify;

                // Notify | API
                TestConfigProperties(ref counter, methodNames, notifyConfiguration.API);

                TestContext.WriteLine($"Tested environment variables: {counter}{Environment.NewLine}");
                TestContext.WriteLine($"Methods: {methodNames.Join()}");
            });
        }

        [TestCaseSource(nameof(GetTestCases))]
        public void WebApiConfiguration_InEnvironmentMode_ForSelectedInvalidVariables_ThrowsExceptions(
            (string CaseId, TestDelegate Logic, string ExpectedErrorMessage) test)
        {
            // Arrange
            s_testConfiguration = GetWebApiConfigurationWith(TestLoaderTypes.InvalidEnvironment_v1);

            // Act & Assert
            Assert.Multiple(() =>
            {
                ArgumentException? exception = Assert.Throws<ArgumentException>(test.Logic);

                string expectedFullMessage = test.ExpectedErrorMessage;
                int leftCurlyBracketIndex = expectedFullMessage.IndexOf("{", StringComparison.Ordinal);
                string expectedTrimmedMessage = expectedFullMessage[..leftCurlyBracketIndex];

                Assert.That(exception?.Message.StartsWith(expectedTrimmedMessage), Is.True,
                    message: $"{test.CaseId}: {exception?.Message}");
            });
        }

        private static IEnumerable<(string CaseId, TestDelegate ActualMethod, string ExpectedErrorMessage)> GetTestCases()
        {
            // Invalid: Not existing
            yield return ("#1", () => s_testConfiguration!.ZGW.API.Key.NotifyNL(), Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: ""
            yield return ("#2", () => s_testConfiguration!.ZGW.Domain.OpenNotificaties(), Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: " "
            yield return ("#3", () => s_testConfiguration!.ZGW.Domain.OpenZaak(), Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: http://domain
            yield return ("#4", () => s_testConfiguration!.ZGW.Domain.OpenKlant(), Resources.Configuration_ERROR_ContainsHttp);
            // Invalid: https://domain
            yield return ("#5", () => s_testConfiguration!.ZGW.Domain.Objecten(), Resources.Configuration_ERROR_ContainsHttp);
            // Invalid: Empty
            yield return ("#6", () => s_testConfiguration!.ZGW.TemplateIds.Sms.ZaakCreate(), Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: Empty
            yield return ("#7", () => s_testConfiguration!.ZGW.TemplateIds.Sms.ZaakUpdate(), Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: 8-4-(2-2)-4-12
            yield return ("#8", () => s_testConfiguration!.ZGW.TemplateIds.Sms.ZaakClose(), Resources.Configuration_ERROR_InvalidTemplateId);
            // Invalid: (9)-4-4-4-12
            yield return ("#9", () => s_testConfiguration!.ZGW.TemplateIds.Sms.TaskAssigned(), Resources.Configuration_ERROR_InvalidTemplateId);
            // Invalid: Special characters
            yield return ("#10", () => s_testConfiguration!.ZGW.TemplateIds.Sms.MessageReceived(), Resources.Configuration_ERROR_InvalidTemplateId);
            // Invalid: Default URI
            yield return ("#11", () => s_testConfiguration!.Notify.API.BaseUrl(), Resources.Configuration_ERROR_InvalidUri);
        }

        [TestCase("1", true)]
        [TestCase("9", false)]
        [TestCase("", false)]
        [TestCase(" ", false)]
        public void IsAllowed_InEnvironmentMode_ForSpecificCaseId_ReturnsExpectedResult(string caseId, bool expectedResult)
        {
            // Arrange
            s_testConfiguration = GetWebApiConfigurationWith(TestLoaderTypes.ValidEnvironment_v1);

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
            s_testConfiguration = GetWebApiConfigurationWith(TestLoaderTypes.InvalidEnvironment_v1);

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
            using var configuration = GetWebApiConfigurationWith(TestLoaderTypes.InvalidEnvironment_v1);

            // Act
            string openKlantApiKey = configuration.ZGW.API.Key.OpenKlant();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(configuration.OMC.Features.Workflow_Version(), Is.EqualTo(1));
                Assert.That(openKlantApiKey, Is.Empty);
            });
        }

        [Test]
        public void OpenKlant_InEnvironmentMode_OmcWorkflowV2_ApiKeyIsRequired()
        {
            // Arrange
            using var configuration = GetWebApiConfigurationWith(TestLoaderTypes.InvalidEnvironment_v2);

            // Act & Assert
            Assert.Multiple(() =>
            {
                Assert.That(configuration.OMC.Features.Workflow_Version(), Is.EqualTo(2));
                ArgumentException? exception = Assert.Throws<ArgumentException>(() => configuration.ZGW.API.Key.OpenKlant());
                Assert.That(exception?.Message, Is.EqualTo(Resources.Configuration_ERROR_ValueNotFoundOrEmpty
                    .Replace("{0}", "ZGW_API_KEY_OPENKLANT")));
            });
        }
        #endregion

        #region Helper methods
        private static void TestConfigProperties(ref int counter, ICollection<string> methodNames, object instance, bool isRecursionEnabled = true)
        {
            const string variableTestErrorMessage =
                $"Most likely the setting or environment variable name was changed in {nameof(WebApiConfiguration)} but not adjusted in {nameof(ConfigurationHandler)}.";

            foreach (MethodInfo method in GetConfigMethods(instance.GetType(), isRecursionEnabled).ToArray())
            {
                counter++;
                methodNames.Add($"{method.Name}()");

                Assert.That(method.Invoke(instance, null), Is.Not.Default, $"{method.Name}: {variableTestErrorMessage}");
            }
        }

        private static IEnumerable<MethodInfo> GetConfigMethods(IReflect currentType, bool isRecursionEnabled)
        {
            IEnumerable<MemberInfo> members = currentType
                .GetMembers(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(member => member.GetCustomAttribute<ConfigAttribute>() != null);

            foreach (MemberInfo member in members)
            {
                // Returning method
                if (member is MethodInfo method)
                {
                    yield return method;
                }

                if (isRecursionEnabled && member is PropertyInfo property)
                {
                    // Traversing recursively to get methods from property
                    foreach (MethodInfo nestedMethod in GetConfigMethods(property.PropertyType, isRecursionEnabled))
                    {
                        yield return nestedMethod;
                    }
                }
            }
        }
        #endregion
    }
}