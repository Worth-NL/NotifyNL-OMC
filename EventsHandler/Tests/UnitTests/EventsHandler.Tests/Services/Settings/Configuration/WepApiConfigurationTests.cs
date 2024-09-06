// © 2023, Worth Systems.

using EventsHandler.Properties;
using EventsHandler.Services.Settings.Attributes;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Utilities._TestHelpers;
using System.Reflection;

#pragma warning disable IDE0008  // Declaration of static types would be too long
// ReSharper disable SuggestVarOrType_SimpleTypes

namespace EventsHandler.UnitTests.Services.Settings.Configuration
{
    [TestFixture]
    public sealed class WepApiConfigurationTests
    {
        private static WebApiConfiguration s_testConfiguration = null!;

        [TearDown]
        public void TestCleanup()
        {
            s_testConfiguration.Dispose();
        }

        #region AppSettings
        [Test]
        public void WebApiConfiguration_Valid_AppSettings_ReturnsNotDefaultValues()
        {
            // Arrange
            s_testConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypes.ValidAppSettings);

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

                // AppSettings | Variables | Objecten
                TestConfigProperties(ref counter, methodNames, variablesSettings.Objecten);

                // AppSettings | Variables | UX Messages
                TestConfigProperties(ref counter, methodNames, variablesSettings.UxMessages);

                TestContext.WriteLine($"Tested environment variables: {counter}{Environment.NewLine}");
                TestContext.WriteLine($"Methods: {string.Join(", ", methodNames)}");
            });
        }
        #endregion

        #region Environment variables
        [Test]
        public void WebApiConfiguration_Valid_EnvironmentVariables_ReturnsNotDefaultValues()
        {
            // Arrange
            s_testConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypes.ValidEnvironment);

            int counter = 0;
            List<string> methodNames = new();

            // Act & Assert
            Assert.Multiple(() =>
            {
                var omcConfiguration = s_testConfiguration.OMC;

                // OMC | Authorization | JWT
                TestConfigProperties(ref counter, methodNames, omcConfiguration.Authorization.JWT);

                // OMC | API | BaseUrl
                TestConfigProperties(ref counter, methodNames, omcConfiguration.API.BaseUrl);

                // OMC | Features
                TestConfigProperties(ref counter, methodNames, omcConfiguration.Features);
                
                var userConfiguration = s_testConfiguration.User;

                // User | Authorization | JWT
                TestConfigProperties(ref counter, methodNames, userConfiguration.Authorization.JWT);

                // User | API | Key
                TestConfigProperties(ref counter, methodNames, userConfiguration.API.Key);

                // User | Domain
                TestConfigProperties(ref counter, methodNames, userConfiguration.Domain);

                // User | Templates (Email + SMS)
                TestConfigProperties(ref counter, methodNames, userConfiguration.TemplateIds.Email);
                TestConfigProperties(ref counter, methodNames, userConfiguration.TemplateIds.Sms);

                // User | Whitelist
                TestConfigProperties(ref counter, methodNames, userConfiguration.Whitelist);

                TestContext.WriteLine($"Tested environment variables: {counter}{Environment.NewLine}");
                TestContext.WriteLine($"Methods: {string.Join(", ", methodNames)}");
            });
        }

        [TestCaseSource(nameof(GetTestCases))]
        public void WebApiConfiguration_InEnvironmentMode_ForSelectedInvalidVariables_ThrowsExceptions(
            (string CaseId, TestDelegate Logic, string ExpectedErrorMessage) test)
        {
            // Arrange
            s_testConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypes.InvalidEnvironment);

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
            yield return ("#1", () => s_testConfiguration.User.API.Key.NotifyNL(), Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: http://domain
            yield return ("#2", () => s_testConfiguration.User.Domain.OpenZaak(), Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: http://domain
            yield return ("#3", () => s_testConfiguration.User.Domain.OpenKlant(), Resources.Configuration_ERROR_ContainsHttp);
            // Invalid: https://domain
            yield return ("#4", () => s_testConfiguration.User.Domain.Objecten(), Resources.Configuration_ERROR_ContainsHttp);
            // Invalid: domain/api/v1/typen
            yield return ("#5", () => s_testConfiguration.User.Domain.ObjectTypen(), Resources.Configuration_ERROR_ContainsEndpoint);
            // Invalid: Empty
            yield return ("#6", () => s_testConfiguration.User.TemplateIds.Sms.ZaakCreate(), Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: Empty
            yield return ("#7", () => s_testConfiguration.User.TemplateIds.Sms.ZaakUpdate(), Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: 8-4-(2-2)-4-12
            yield return ("#8", () => s_testConfiguration.User.TemplateIds.Sms.ZaakClose(), Resources.Configuration_ERROR_InvalidTemplateId);
            // Invalid: (9)-4-4-4-12
            yield return ("#9", () => s_testConfiguration.User.TemplateIds.Sms.TaskAssigned(), Resources.Configuration_ERROR_InvalidTemplateId);
            // Invalid: Special characters
            yield return ("#10", () => s_testConfiguration.User.TemplateIds.Sms.MessageReceived(), Resources.Configuration_ERROR_InvalidTemplateId);
            // Invalid: Default URI
            yield return ("#11", () => s_testConfiguration.OMC.API.BaseUrl.NotifyNL(), Resources.Configuration_ERROR_InvalidUri);
        }

        [TestCase("1", true)]
        [TestCase("9", false)]
        [TestCase("", false)]
        [TestCase(" ", false)]
        public void IsAllowed_InEnvironmentMode_ForSpecificCaseId_ReturnsExpectedResult(string caseId, bool expectedResult)
        {
            // Arrange
            s_testConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypes.ValidEnvironment);

            // Act
            var whitelistedIDs = s_testConfiguration.User.Whitelist.ZaakCreate_IDs();
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
            s_testConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypes.InvalidEnvironment);

            // Act
            var whitelistedIDs = s_testConfiguration.User.Whitelist.ZaakCreate_IDs();
            bool isAllowed = whitelistedIDs.IsAllowed("1");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(whitelistedIDs.Count, Is.Zero);
                Assert.That(isAllowed, Is.False);
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