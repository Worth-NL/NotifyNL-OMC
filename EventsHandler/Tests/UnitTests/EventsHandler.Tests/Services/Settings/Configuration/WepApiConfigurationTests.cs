// © 2023, Worth Systems.

using EventsHandler.Properties;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Utilities._TestHelpers;

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

        #region Environment variables
        #pragma warning disable IDE0008  // Explicit types are too long and not necessary to be used here
        // ReSharper disable SuggestVarOrType_SimpleTypes
        [Test]
        public void WebApiConfiguration_InEnvironmentMode_ForAllValidVariables_ReadsProperties()
        {
            // Arrange
            s_testConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypes.ValidEnvironment);

            // Act & Assert
            Assert.Multiple(() =>
            {
                const string variableTestErrorMessage =
                    $"Most likely the environment variable name was changed in {nameof(WebApiConfiguration)} " +
                    $"but not adjusted in {nameof(ConfigurationHandler)}.GetEnvironmentLoader(bool).";

                var omcConfiguration = s_testConfiguration.OMC;
                var userConfiguration = s_testConfiguration.User;

                // TODO: Use reflexion to get all properties and test them dynamically
                // Authorization | JWT | Notify
                var notifyJwt = omcConfiguration.Authorization.JWT;
                Assert.That(notifyJwt.Secret(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);
                Assert.That(notifyJwt.Issuer(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);
                Assert.That(notifyJwt.Audience(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);
                Assert.That(notifyJwt.ExpiresInMin(), Is.Not.Zero, message: variableTestErrorMessage);
                Assert.That(notifyJwt.UserId(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);
                Assert.That(notifyJwt.UserName(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);

                // API | BaseUrl
                Assert.That(omcConfiguration.API.BaseUrl.NotifyNL(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);

                // Authorization | JWT | User
                var userJwt = userConfiguration.Authorization.JWT;
                Assert.That(userJwt.Secret(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);
                Assert.That(userJwt.Issuer(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);
                Assert.That(userJwt.Audience(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);
                Assert.That(userJwt.ExpiresInMin(), Is.Not.Zero, message: variableTestErrorMessage);
                Assert.That(userJwt.UserId(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);
                Assert.That(userJwt.UserName(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);

                // Authorization | Key
                var key = userConfiguration.API.Key;
                Assert.That(key.OpenKlant_2(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);
                Assert.That(key.Objecten(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);
                Assert.That(key.ObjectTypen(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);
                Assert.That(key.NotifyNL(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);

                // API | Domain
                var apiDomain = userConfiguration.Domain;
                Assert.That(apiDomain.OpenNotificaties(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);
                Assert.That(apiDomain.OpenZaak(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);
                Assert.That(apiDomain.OpenKlant(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);
                Assert.That(apiDomain.Objecten(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);
                Assert.That(apiDomain.ObjectTypen(), Is.Not.Null.Or.Empty, message: variableTestErrorMessage);

                // Templates
                var templateIds = userConfiguration.TemplateIds;
                Assert.That(templateIds.Email.ZaakCreate(), Is.Not.Empty, message: variableTestErrorMessage);
                Assert.That(templateIds.Email.ZaakUpdate(), Is.Not.Empty, message: variableTestErrorMessage);
                Assert.That(templateIds.Email.ZaakClose(), Is.Not.Empty, message: variableTestErrorMessage);
                Assert.That(templateIds.Email.TaskAssigned(), Is.Not.Empty, message: variableTestErrorMessage);
                Assert.That(templateIds.Email.MessageReceived(), Is.Not.Empty, message: variableTestErrorMessage);

                Assert.That(templateIds.Sms.ZaakCreate(), Is.Not.Empty, message: variableTestErrorMessage);
                Assert.That(templateIds.Sms.ZaakUpdate(), Is.Not.Empty, message: variableTestErrorMessage);
                Assert.That(templateIds.Sms.ZaakClose(), Is.Not.Empty, message: variableTestErrorMessage);
                Assert.That(templateIds.Sms.TaskAssigned(), Is.Not.Empty, message: variableTestErrorMessage);
                Assert.That(templateIds.Sms.MessageReceived(), Is.Not.Empty, message: variableTestErrorMessage);

                // Whitelist
                var whitelist = userConfiguration.Whitelist;
                Assert.That(whitelist.ZaakCreate_IDs().Count, Is.Not.Zero, message: variableTestErrorMessage);
                Assert.That(whitelist.ZaakUpdate_IDs().Count, Is.Not.Zero, message: variableTestErrorMessage);
                Assert.That(whitelist.ZaakClose_IDs().Count, Is.Not.Zero, message: variableTestErrorMessage);
                Assert.That(whitelist.TaskAssigned_IDs().Count, Is.Not.Zero, message: variableTestErrorMessage);
                Assert.That(whitelist.DecisionMade_IDs().Count, Is.Not.Zero, message: variableTestErrorMessage);
                Assert.That(whitelist.Message_Allowed(), Is.Not.False, message: variableTestErrorMessage);
                Assert.That(whitelist.TaskObjectType_Uuid(), Is.Not.Empty, message: variableTestErrorMessage);
                Assert.That(whitelist.MessageObjectType_Uuid(), Is.Not.Empty, message: variableTestErrorMessage);
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
        #pragma warning restore IDE0008
        #endregion
    }
}