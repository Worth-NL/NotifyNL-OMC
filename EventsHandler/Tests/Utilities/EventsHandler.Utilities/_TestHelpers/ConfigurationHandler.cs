// © 2023, Worth Systems.

using EventsHandler.Configuration;
using EventsHandler.Services.DataLoading;
using EventsHandler.Services.DataLoading.Interfaces;
using EventsHandler.Services.DataLoading.Strategy.Manager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EventsHandler.Utilities._TestHelpers
{
    /// <summary>
    /// The configuration handler used for test project.
    /// </summary>
    internal static class ConfigurationHandler
    {
        #region Loading services (DAO)
        /// <summary>
        /// Gets the test <see cref="IConfiguration"/>.
        /// </summary>
        internal static IConfiguration GetConfiguration()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .AddEnvironmentVariables()
                .Build();

            return configuration;
        }

        /// <summary>
        /// Gets the test <see cref="ConfigurationLoader"/>.
        /// </summary>
        internal static ILoadingService GetConfigurationLoader()
        {
            return new ConfigurationLoader(GetConfiguration());
        }

        internal static ILoadingService GetEnvironmentLoader(bool isValid = true)
        {
            var mockedEnvironmentLoader = new Mock<ILoadingService>();

            const string testValue = "Test";
            const string testDomain = "test.domain";
            const string testTempId = "00000000-0000-0000-0000-000000000000";

            #region GetData<T>() mocking
            Dictionary<string /* Key */, string /* Value */> keyValueMapping = new()
            {
                { "NOTIFY_AUTHORIZATION_JWT_SECRET",   isValid ? testValue : string.Empty },
                { "NOTIFY_AUTHORIZATION_JWT_ISSUER",   isValid ? testValue : string.Empty },
                { "NOTIFY_AUTHORIZATION_JWT_AUDIENCE", isValid ? testValue : string.Empty },
                { "NOTIFY_AUTHORIZATION_JWT_USERID",   isValid ? testValue : string.Empty },
                { "NOTIFY_AUTHORIZATION_JWT_USERNAME", isValid ? testValue : string.Empty },
                
                { "NOTIFY_API_BASEURL",                isValid ? "https://www.test.nl/" : string.Empty },
                
                { "USER_AUTHORIZATION_JWT_SECRET",     isValid ? testValue : string.Empty },
                { "USER_AUTHORIZATION_JWT_ISSUER",     isValid ? testValue : string.Empty },
                { "USER_AUTHORIZATION_JWT_AUDIENCE",   isValid ? testValue : string.Empty },
                { "USER_AUTHORIZATION_JWT_USERID",     isValid ? testValue : string.Empty },
                { "USER_AUTHORIZATION_JWT_USERNAME",   isValid ? testValue : string.Empty },

                { "USER_API_KEY_NOTIFYNL",             isValid ? testValue : string.Empty },
                { "USER_API_KEY_OBJECTEN",             isValid ? testValue : string.Empty },

                { "USER_DOMAIN_OPENNOTIFICATIES",      isValid ? testDomain : string.Empty },
                { "USER_DOMAIN_OPENZAAK",              isValid ? testDomain : string.Empty },
                { "USER_DOMAIN_OPENKLANT",             isValid ? testDomain : string.Empty },
                { "USER_DOMAIN_OBJECTEN",              isValid ? testDomain : string.Empty },
                { "USER_DOMAIN_OBJECTTYPEN",           isValid ? testDomain : string.Empty },

                { "USER_TEMPLATEIDS_SMS_ZAAKCREATE",   isValid ? testTempId : string.Empty },
                { "USER_TEMPLATEIDS_SMS_ZAAKUPDATE",   isValid ? testTempId : string.Empty },
                { "USER_TEMPLATEIDS_SMS_ZAAKCLOSE",    isValid ? testTempId : string.Empty },
                { "USER_TEMPLATEIDS_EMAIL_ZAAKCREATE", isValid ? testTempId : string.Empty },
                { "USER_TEMPLATEIDS_EMAIL_ZAAKUPDATE", isValid ? testTempId : string.Empty },
                { "USER_TEMPLATEIDS_EMAIL_ZAAKCLOSE",  isValid ? testTempId : string.Empty }
            };

            foreach (KeyValuePair<string, string> keyValue in keyValueMapping)
            {
                mockedEnvironmentLoader
                    .Setup(mock => mock.GetData<string>(keyValue.Key))
                    .Returns(keyValue.Value);
            }

            mockedEnvironmentLoader
                .Setup(mock => mock.GetData<ushort>("NOTIFY_AUTHORIZATION_JWT_EXPIRESINMIN"))
                .Returns((ushort)(isValid ? 60 : 0));

            mockedEnvironmentLoader
                .Setup(mock => mock.GetData<ushort>("USER_AUTHORIZATION_JWT_EXPIRESINMIN"))
                .Returns((ushort)(isValid ? 60 : 0));
            #endregion

            #region GetPathWithNode() mocking
            (string Path, string Node, string ResultPath)[] testData =
            {
                ("Notify", "Authorization", "NOTIFY_AUTHORIZATION"),
                ("User",   "Authorization", "USER_AUTHORIZATION"),

                ("NOTIFY_AUTHORIZATION",     "JWT",          "NOTIFY_AUTHORIZATION_JWT"),
                ("NOTIFY_AUTHORIZATION_JWT", "Secret",       "NOTIFY_AUTHORIZATION_JWT_SECRET"),
                ("NOTIFY_AUTHORIZATION_JWT", "Issuer",       "NOTIFY_AUTHORIZATION_JWT_ISSUER"),
                ("NOTIFY_AUTHORIZATION_JWT", "Audience",     "NOTIFY_AUTHORIZATION_JWT_AUDIENCE"),
                ("NOTIFY_AUTHORIZATION_JWT", "ExpiresInMin", "NOTIFY_AUTHORIZATION_JWT_EXPIRESINMIN"),
                ("NOTIFY_AUTHORIZATION_JWT", "UserId",       "NOTIFY_AUTHORIZATION_JWT_USERID"),
                ("NOTIFY_AUTHORIZATION_JWT", "UserName",     "NOTIFY_AUTHORIZATION_JWT_USERNAME"),

                ("Notify",     "API",     "NOTIFY_API"),
                ("NOTIFY_API", "BaseUrl", "NOTIFY_API_BASEURL"),

                ("USER_AUTHORIZATION",     "JWT",          "USER_AUTHORIZATION_JWT"),
                ("USER_AUTHORIZATION_JWT", "Secret",       "USER_AUTHORIZATION_JWT_SECRET"),
                ("USER_AUTHORIZATION_JWT", "Issuer",       "USER_AUTHORIZATION_JWT_ISSUER"),
                ("USER_AUTHORIZATION_JWT", "Audience",     "USER_AUTHORIZATION_JWT_AUDIENCE"),
                ("USER_AUTHORIZATION_JWT", "ExpiresInMin", "USER_AUTHORIZATION_JWT_EXPIRESINMIN"),
                ("USER_AUTHORIZATION_JWT", "UserId",       "USER_AUTHORIZATION_JWT_USERID"),
                ("USER_AUTHORIZATION_JWT", "UserName",     "USER_AUTHORIZATION_JWT_USERNAME"),

                ("User",         "API",      "USER_API"),
                ("USER_API",     "Key",      "USER_API_KEY"),
                ("USER_API_KEY", "NotifyNL", "USER_API_KEY_NOTIFYNL"),
                ("USER_API_KEY", "Objecten", "USER_API_KEY_OBJECTEN"),

                ("User",        "Domain",           "USER_DOMAIN"),
                ("USER_DOMAIN", "OpenNotificaties", "USER_DOMAIN_OPENNOTIFICATIES"),
                ("USER_DOMAIN", "OpenZaak",         "USER_DOMAIN_OPENZAAK"),
                ("USER_DOMAIN", "OpenKlant",        "USER_DOMAIN_OPENKLANT"),
                ("USER_DOMAIN", "Objecten",         "USER_DOMAIN_OBJECTEN"),
                ("USER_DOMAIN", "ObjectTypen",      "USER_DOMAIN_OBJECTTYPEN"),

                ("User",                   "TemplateIds", "USER_TEMPLATEIDS"),
                ("USER_TEMPLATEIDS",       "Sms",         "USER_TEMPLATEIDS_SMS"),
                ("USER_TEMPLATEIDS_SMS",   "ZaakCreate",  "USER_TEMPLATEIDS_SMS_ZAAKCREATE"),
                ("USER_TEMPLATEIDS_SMS",   "ZaakUpdate",  "USER_TEMPLATEIDS_SMS_ZAAKUPDATE"),
                ("USER_TEMPLATEIDS_SMS",   "ZaakClose",   "USER_TEMPLATEIDS_SMS_ZAAKCLOSE"),

                ("USER_TEMPLATEIDS",       "Email",      "USER_TEMPLATEIDS_EMAIL"),
                ("USER_TEMPLATEIDS_EMAIL", "ZaakCreate", "USER_TEMPLATEIDS_EMAIL_ZAAKCREATE"),
                ("USER_TEMPLATEIDS_EMAIL", "ZaakUpdate", "USER_TEMPLATEIDS_EMAIL_ZAAKUPDATE"),
                ("USER_TEMPLATEIDS_EMAIL", "ZaakClose",  "USER_TEMPLATEIDS_EMAIL_ZAAKCLOSE"),
            };

            foreach ((string Path, string Node, string ResultPath) data in testData)
            {
                mockedEnvironmentLoader
                    .Setup(mock => mock.GetPathWithNode(data.Path, data.Node))
                    .Returns(data.ResultPath);
            }
            #endregion

            return mockedEnvironmentLoader.Object;
        }
        #endregion

        #region Web API Configuration
        internal static WebApiConfiguration GetWebApiConfiguration(ILoadingService loadingService)
        {
            // Service Provider (does not require mocking)
            using ServiceProvider serviceProvider = new ServiceCollection().BuildServiceProvider();
            
            // Loaders Context
            var loadersContext = new LoadersContext(serviceProvider, loadingService);

            // Web API Configuration
            return new WebApiConfiguration(loadersContext);
        }
        #endregion
    }
}