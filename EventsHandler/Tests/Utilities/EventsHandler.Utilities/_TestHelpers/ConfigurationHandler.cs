// © 2023, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Services.Settings;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Settings.DAO.Interfaces;
using EventsHandler.Services.Settings.Enums;
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
        internal const string TestTypeUuid = "0236e468-2ad8-43d6-a723-219cb22acb37";

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
            var mockedEnvironmentReader = new Mock<IEnvironment>();

            const string testString = "xyz";
            const string testArray = "1, 2, 3";
            const string testUshort = "60";
            const string testGuid = "01234567-89ab-cdef-1234-567890123456";
            const string testBool = "true";
            const string testDomain = "test.domain";

            // NOTE: Update the keys manually if the structure of the WebApiConfiguration change
            #region GetEnvironmentVariable<T>() mocking
            Dictionary<string /* Key */, string? /* Value */> keyValueMapping = new()
            {
                { "OMC_AUTHORIZATION_JWT_SECRET",        GetTestValue(isValid, testString) },
                { "OMC_AUTHORIZATION_JWT_ISSUER",        GetTestValue(isValid, testString) },
                { "OMC_AUTHORIZATION_JWT_AUDIENCE",      GetTestValue(isValid, testString) },
                { "OMC_AUTHORIZATION_JWT_EXPIRESINMIN",  GetTestValue(isValid, testUshort) },
                { "OMC_AUTHORIZATION_JWT_USERID",        GetTestValue(isValid, testString) },
                { "OMC_AUTHORIZATION_JWT_USERNAME",      GetTestValue(isValid, testString) },

                { "OMC_API_BASEURL_NOTIFYNL",            GetTestValue(isValid, "https://www.test.notify.nl/", DefaultValues.Models.EmptyUri.ToString()) },

                { "USER_AUTHORIZATION_JWT_SECRET",       GetTestValue(isValid, testString) },
                { "USER_AUTHORIZATION_JWT_ISSUER",       GetTestValue(isValid, testString) },
                { "USER_AUTHORIZATION_JWT_AUDIENCE",     GetTestValue(isValid, testString) },
                { "USER_AUTHORIZATION_JWT_EXPIRESINMIN", GetTestValue(isValid, testUshort) },
                { "USER_AUTHORIZATION_JWT_USERID",       GetTestValue(isValid, testString) },
                { "USER_AUTHORIZATION_JWT_USERNAME",     GetTestValue(isValid, testString) },

                { "USER_API_KEY_OPENKLANT_2",            GetTestValue(isValid, testString) },
                { "USER_API_KEY_OBJECTEN",               GetTestValue(isValid, testString) },
                { "USER_API_KEY_OBJECTTYPEN",            GetTestValue(isValid, testString) },
                { "USER_API_KEY_NOTIFYNL",               GetTestValue(isValid, testString) },

                { "USER_DOMAIN_OPENNOTIFICATIES",        GetTestValue(isValid, testDomain) },
                { "USER_DOMAIN_OPENZAAK",                GetTestValue(isValid, testDomain) },
                { "USER_DOMAIN_OPENKLANT",               GetTestValue(isValid, testDomain, "http://domain") },
                { "USER_DOMAIN_OBJECTEN",                GetTestValue(isValid, testDomain, "https://domain") },
                { "USER_DOMAIN_OBJECTTYPEN",             GetTestValue(isValid, testDomain, "domain/api/v1/typen") },

                { "USER_TEMPLATEIDS_EMAIL_ZAAKCREATE",   GetTestValue(isValid, testGuid) },
                { "USER_TEMPLATEIDS_EMAIL_ZAAKUPDATE",   GetTestValue(isValid, testGuid) },
                { "USER_TEMPLATEIDS_EMAIL_ZAAKCLOSE",    GetTestValue(isValid, testGuid) },
                { "USER_TEMPLATEIDS_EMAIL_TASKASSIGNED", GetTestValue(isValid, testGuid) },
                { "USER_TEMPLATEIDS_EMAIL_DECISIONMADE", GetTestValue(isValid, testGuid) },

                { "USER_TEMPLATEIDS_SMS_ZAAKCREATE",     GetTestValue(isValid, testGuid) },
                { "USER_TEMPLATEIDS_SMS_ZAAKUPDATE",     GetTestValue(isValid, testGuid) },
                { "USER_TEMPLATEIDS_SMS_ZAAKCLOSE",      GetTestValue(isValid, testGuid, "12345678-1234-12-34-1234-123456789012") },
                { "USER_TEMPLATEIDS_SMS_TASKASSIGNED",   GetTestValue(isValid, testGuid, "123456789-1234-1234-1234-123456789012") },
                { "USER_TEMPLATEIDS_SMS_DECISIONMADE",   GetTestValue(isValid, testGuid, "!2345678-1234-12-34-1234-123456789*12") },

                { "USER_WHITELIST_ZAAKCREATE_IDS",       GetTestValue(isValid, testArray) },
                { "USER_WHITELIST_ZAAKUPDATE_IDS",       GetTestValue(isValid, testArray) },
                { "USER_WHITELIST_ZAAKCLOSE_IDS",        GetTestValue(isValid, testArray) },
                { "USER_WHITELIST_TASKASSIGNED_IDS",     GetTestValue(isValid, testArray) },
                { "USER_WHITELIST_DECISIONMADE_IDS",     GetTestValue(isValid, testArray) },
                { "USER_WHITELIST_MESSAGE_ALLOWED",      GetTestValue(isValid, testBool)  },
                { "USER_WHITELIST_TASKTYPE_UUID",        GetTestValue(isValid, TestTypeUuid)  },
                { "USER_WHITELIST_MESSAGETYPE_UUID",     GetTestValue(isValid, TestTypeUuid)  }
            };

            static string? GetTestValue(bool isValid, string validString, string? invalidString = null)
            {
                return isValid ? validString : invalidString;  // Simulates behavior of real Environment.GetEnvironmentVariable(string)
            }

            foreach (KeyValuePair<string, string?> keyValue in keyValueMapping)
            {
                mockedEnvironmentReader
                    .Setup(mock => mock.GetEnvironmentVariable(keyValue.Key))
                    .Returns(keyValue.Value);
            }
            #endregion

            return new EnvironmentLoader
            {
                Environment = mockedEnvironmentReader.Object
            };
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

        internal static WebApiConfiguration GetValidEnvironmentConfiguration()
        {
            return GetWebApiConfiguration(LoaderTypes.Environment, isValid: true);
        }
        #endregion
    }
}