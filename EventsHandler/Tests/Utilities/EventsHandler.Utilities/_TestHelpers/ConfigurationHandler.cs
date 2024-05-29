// © 2023, Worth Systems.

using EventsHandler.Configuration;
using EventsHandler.Services.DataLoading;
using EventsHandler.Services.DataLoading.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MoqExt;

namespace EventsHandler.Utilities._TestHelpers
{
    /// <summary>
    /// The configuration handler used for test project.
    /// </summary>
    internal static class ConfigurationHandler
    {
        /// <summary>
        /// Gets the test <see cref="IConfiguration"/> with (re)created "appsettings.Test.json" and environment variables.
        /// </summary>
        internal static IConfiguration GetConfiguration()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .Build();

            return configuration;
        }

        #region ILoadingService mocks
        /// <summary>
        /// Gets the mocked <see cref="AppSettingsLoader"/>.
        /// </summary>
        private static AppSettingsLoader GetAppSettingsLoader(bool isValid = true)
        {
            return isValid
                ? new AppSettingsLoader(GetConfiguration())
                : new AppSettingsLoader(new Mock<IConfiguration>().Object);
        }
        
        /// <summary>
        /// Gets the mocked <see cref="EnvironmentLoader"/>.
        /// </summary>
        private static EnvironmentLoader GetEnvironmentLoader(bool isValid = true)
        {
            var mockedEnvironmentLoader = new Mock<EnvironmentLoader>();

            const string testValue = "Test";
            const string testDomain = "test.domain";
            const string testTempId = "00000000-0000-0000-0000-000000000000";

            static string GetTestValue(bool isValid, string validString, string? invalidString = null)
            {
                return isValid ? validString : invalidString ?? string.Empty;
            }

            // NOTE: Update the keys manually if the structure of the WebApiConfiguration change
            #region GetData<T>() mocking
            Dictionary<string /* Key */, string /* Value */> keyValueMapping = new()
            {
                { "OMC_AUTHORIZATION_JWT_SECRET",      GetTestValue(isValid, testValue) },
                { "OMC_AUTHORIZATION_JWT_ISSUER",      GetTestValue(isValid, testValue) },
                { "OMC_AUTHORIZATION_JWT_AUDIENCE",    GetTestValue(isValid, testValue) },
                { "OMC_AUTHORIZATION_JWT_USERID",      GetTestValue(isValid, testValue) },
                { "OMC_AUTHORIZATION_JWT_USERNAME",    GetTestValue(isValid, testValue) },

                { "OMC_API_BASEURL_NOTIFYNL",          GetTestValue(isValid, "https://www.test.nl/") },
                
                { "USER_AUTHORIZATION_JWT_SECRET",     GetTestValue(isValid, testValue) },
                { "USER_AUTHORIZATION_JWT_ISSUER",     GetTestValue(isValid, testValue) },
                { "USER_AUTHORIZATION_JWT_AUDIENCE",   GetTestValue(isValid, testValue) },
                { "USER_AUTHORIZATION_JWT_USERID",     GetTestValue(isValid, testValue) },
                { "USER_AUTHORIZATION_JWT_USERNAME",   GetTestValue(isValid, testValue) },

                { "USER_API_KEY_NOTIFYNL",             GetTestValue(isValid, testValue) },
                { "USER_API_KEY_OBJECTEN",             GetTestValue(isValid, testValue) },

                { "USER_DOMAIN_OPENNOTIFICATIES",      GetTestValue(isValid, testDomain) },
                { "USER_DOMAIN_OPENZAAK",              GetTestValue(isValid, testDomain) },
                { "USER_DOMAIN_OPENKLANT",             GetTestValue(isValid, testDomain, "http://domain") },
                { "USER_DOMAIN_OBJECTEN",              GetTestValue(isValid, testDomain, "https://domain") },
                { "USER_DOMAIN_OBJECTTYPEN",           GetTestValue(isValid, testDomain, "domain/api/v1/typen") },

                { "USER_TEMPLATEIDS_SMS_ZAAKCREATE",   GetTestValue(isValid, testTempId) },
                { "USER_TEMPLATEIDS_SMS_ZAAKUPDATE",   GetTestValue(isValid, testTempId, "12345678-1234-12-34-1234-123456789012") },
                { "USER_TEMPLATEIDS_SMS_ZAAKCLOSE",    GetTestValue(isValid, testTempId, "123456789-1234-1234-1234-123456789012") },
                { "USER_TEMPLATEIDS_EMAIL_ZAAKCREATE", GetTestValue(isValid, testTempId) },
                { "USER_TEMPLATEIDS_EMAIL_ZAAKUPDATE", GetTestValue(isValid, testTempId) },
                { "USER_TEMPLATEIDS_EMAIL_ZAAKCLOSE",  GetTestValue(isValid, testTempId) }
            };

            foreach (KeyValuePair<string, string> keyValue in keyValueMapping)
            {
                mockedEnvironmentLoader
                    .Setup(mock => mock.GetData<string>(keyValue.Key))
                    .Returns(keyValue.Value);
            }

            mockedEnvironmentLoader
                .Setup(mock => mock.GetData<ushort>("OMC_AUTHORIZATION_JWT_EXPIRESINMIN"))
                .Returns((ushort)(isValid ? 60 : 0));

            mockedEnvironmentLoader
                .Setup(mock => mock.GetData<ushort>("USER_AUTHORIZATION_JWT_EXPIRESINMIN"))
                .Returns((ushort)(isValid ? 60 : 0));
            #endregion

            // NOTE: Update the keys manually if the structure of the WebApiConfiguration change
            #region GetPathWithNode() mocking
            (string Path, string Node, string ResultPath)[] testData =
            {
                ("OMC",  "Authorization", "OMC_AUTHORIZATION"),
                ("User", "Authorization", "USER_AUTHORIZATION"),

                ("OMC_AUTHORIZATION",     "JWT",          "OMC_AUTHORIZATION_JWT"),
                ("OMC_AUTHORIZATION_JWT", "Secret",       "OMC_AUTHORIZATION_JWT_SECRET"),
                ("OMC_AUTHORIZATION_JWT", "Issuer",       "OMC_AUTHORIZATION_JWT_ISSUER"),
                ("OMC_AUTHORIZATION_JWT", "Audience",     "OMC_AUTHORIZATION_JWT_AUDIENCE"),
                ("OMC_AUTHORIZATION_JWT", "ExpiresInMin", "OMC_AUTHORIZATION_JWT_EXPIRESINMIN"),
                ("OMC_AUTHORIZATION_JWT", "UserId",       "OMC_AUTHORIZATION_JWT_USERID"),
                ("OMC_AUTHORIZATION_JWT", "UserName",     "OMC_AUTHORIZATION_JWT_USERNAME"),

                ("OMC",             "API",      "OMC_API"),
                ("OMC_API",         "BaseUrl",  "OMC_API_BASEURL"),
                ("OMC_API_BASEURL", "NotifyNL", "OMC_API_BASEURL_NOTIFYNL"),

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
        internal static WebApiConfiguration GetWebApiConfiguration(ServiceCollection? serviceCollection = null)
        {
            // IServiceCollection
            serviceCollection ??= new ServiceCollection();

            // IServiceProvider
            var serviceProvider = new MockingContext(serviceCollection);

            // Web API Configuration
            return new WebApiConfiguration(serviceProvider);
        }

        internal static WebApiConfiguration GetWebApiConfiguration(LoaderTypes loaderType, bool isValid)
        {
            // IServiceCollection
            var serviceCollection = new ServiceCollection();

            // ILoaderService
            switch (loaderType)
            {
                case LoaderTypes.AppSettings:
                    serviceCollection.AddSingleton(GetAppSettingsLoader(isValid));
                    break;
                
                case LoaderTypes.Environment:
                    serviceCollection.AddSingleton(GetEnvironmentLoader(isValid));
                    break;
            }

            // Remaining components of Web API Configuration
            return GetWebApiConfiguration(serviceCollection);
        }

        internal static WebApiConfiguration GetValidAppSettingsConfiguration()
        {
            return GetWebApiConfiguration(LoaderTypes.AppSettings, isValid: true);
        }
        #endregion
    }
}