// © 2023, Worth Systems.

using EventsHandler.Configuration;
using EventsHandler.Properties;
using EventsHandler.Utilities._TestHelpers;

namespace EventsHandler.UnitTests.Configuration
{
    [TestFixture]
    public sealed class WepApiConfigurationTests
    {
        #region Environment variables
        [Test]
        public void WebApiConfiguration_InEnvironmentMode_ForAllValidVariables_ReadsProperties()
        {
            WebApiConfiguration configuration = ConfigurationHandler.GetWebApiConfiguration(
                ConfigurationHandler.GetEnvironmentLoader(isValid: true));

            // Assert
            Assert.Multiple(() =>
            {
                #pragma warning disable IDE0008  // Explicit types are too long and not necessary to be used here
                                                 // ReSharper disable SuggestVarOrType_SimpleTypes
                // Authorization | JWT | Notify
                var notifyJwt = configuration.Notify.Authorization.JWT;
                Assert.That(notifyJwt.Secret(), Is.Not.Null.Or.Empty);
                Assert.That(notifyJwt.Issuer(), Is.Not.Null.Or.Empty);
                Assert.That(notifyJwt.Audience(), Is.Not.Null.Or.Empty);
                Assert.That(notifyJwt.ExpiresInMin(), Is.Not.Zero);
                Assert.That(notifyJwt.UserId(), Is.Not.Null.Or.Empty);
                Assert.That(notifyJwt.UserName(), Is.Not.Null.Or.Empty);

                // API | BaseUrl
                Assert.That(configuration.Notify.API.BaseUrl(), Is.Not.Null.Or.Empty);

                // Authorization | JWT | User
                var userJwt = configuration.User.Authorization.JWT;
                Assert.That(userJwt.Secret(), Is.Not.Null.Or.Empty);
                Assert.That(userJwt.Issuer(), Is.Not.Null.Or.Empty);
                Assert.That(userJwt.Audience(), Is.Not.Null.Or.Empty);
                Assert.That(userJwt.ExpiresInMin(), Is.Not.Zero);
                Assert.That(userJwt.UserId(), Is.Not.Null.Or.Empty);
                Assert.That(userJwt.UserName(), Is.Not.Null.Or.Empty);

                // Authorization | Key
                var key = configuration.User.API.Key;
                Assert.That(key.NotifyNL(), Is.Not.Null.Or.Empty);
                Assert.That(key.Objecten(), Is.Not.Null.Or.Empty);

                // API | Domain
                var apiDomain = configuration.User.Domain;
                Assert.That(apiDomain.OpenNotificaties(), Is.Not.Null.Or.Empty);
                Assert.That(apiDomain.OpenZaak(), Is.Not.Null.Or.Empty);
                Assert.That(apiDomain.OpenKlant(), Is.Not.Null.Or.Empty);
                Assert.That(apiDomain.Objecten(), Is.Not.Null.Or.Empty);
                Assert.That(apiDomain.ObjectTypen(), Is.Not.Null.Or.Empty);

                // Templates
                var templateIds = configuration.User.TemplateIds;
                Assert.That(templateIds.Email.ZaakCreate(), Is.Not.Null.Or.Empty);
                Assert.That(templateIds.Email.ZaakUpdate(), Is.Not.Null.Or.Empty);
                Assert.That(templateIds.Email.ZaakClose(), Is.Not.Null.Or.Empty);
                Assert.That(templateIds.Sms.ZaakCreate(), Is.Not.Null.Or.Empty);
                Assert.That(templateIds.Sms.ZaakUpdate(), Is.Not.Null.Or.Empty);
                Assert.That(templateIds.Sms.ZaakClose(), Is.Not.Null.Or.Empty);
                #pragma warning restore IDE0008
            });
        }

        [TestCaseSource(nameof(GetTestCases))]
        public void WebApiConfiguration_InEnvironmentMode_ForSelectedInvalidVariables_ThrowsExceptions(
            (string CaseId, TestDelegate Logic, string ExpectedErrorMessage) test)
        {
            // Act & Assert
            ArgumentException? exception = Assert.Throws<ArgumentException>(test.Logic);  
            Assert.That(exception?.Message.StartsWith(test.ExpectedErrorMessage), Is.True,
                message: $"{test.CaseId}: {exception?.Message}");
        }

        private static IEnumerable<(string CaseId, TestDelegate ActualMethod, string ExpectedErrorMessage)> GetTestCases()
        {
            WebApiConfiguration configuration = ConfigurationHandler.GetWebApiConfiguration(
                ConfigurationHandler.GetEnvironmentLoader(isValid: false));

            // Invalid: Not existing
            yield return ("#1", () => configuration.User.API.Key.NotifyNL(), Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: Empty
            yield return ("#2", () => configuration.User.Authorization.JWT.Audience(), Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: http://domain
            yield return ("#3", () => configuration.User.Domain.OpenZaak(), Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: http://domain
            yield return ("#4", () => configuration.User.Domain.OpenKlant(), Resources.Configuration_ERROR_ContainsHttp);
            // Invalid: https://domain
            yield return ("#5", () => configuration.User.Domain.Objecten(), Resources.Configuration_ERROR_ContainsHttp);
            // Invalid: domain/api/v1/typen
            yield return ("#6", () => configuration.User.Domain.ObjectTypen(), Resources.Configuration_ERROR_ContainsEndpoint);
            // Invalid: Whitespace
            yield return ("#7", () => configuration.User.TemplateIds.Sms.ZaakCreate(), Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            // Invalid: 8-4-(2-2)-4-12
            yield return ("#8", () => configuration.User.TemplateIds.Sms.ZaakUpdate(), Resources.Configuration_ERROR_InvalidTemplateId);
            // Invalid: (9)-4-4-4-12
            yield return ("#9", () => configuration.User.TemplateIds.Sms.ZaakClose(), Resources.Configuration_ERROR_InvalidTemplateId);
        }
        #endregion
    }
}