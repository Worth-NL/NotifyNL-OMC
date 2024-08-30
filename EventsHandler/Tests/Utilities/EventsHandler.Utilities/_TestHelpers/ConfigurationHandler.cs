// © 2023, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Services.Settings;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Settings.DAO.Interfaces;
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
        internal const string TestTaskObjectTypeUuid = "0236e468-2ad8-43d6-a723-219cb22acb37";
        internal const string TestMessageObjectTypeUuid = "38327774-7023-4f25-9386-acb0c6f10636";  // TODO: Use something real

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
                { "OMC_AUTHORIZATION_JWT_SECRET",           GetTestValue(isValid, testString) },
                { "OMC_AUTHORIZATION_JWT_ISSUER",           GetTestValue(isValid, testString) },
                { "OMC_AUTHORIZATION_JWT_AUDIENCE",         GetTestValue(isValid, testString) },
                { "OMC_AUTHORIZATION_JWT_EXPIRESINMIN",     GetTestValue(isValid, testUshort) },
                { "OMC_AUTHORIZATION_JWT_USERID",           GetTestValue(isValid, testString) },
                { "OMC_AUTHORIZATION_JWT_USERNAME",         GetTestValue(isValid, testString) },

                { "OMC_API_BASEURL_NOTIFYNL",               GetTestValue(isValid, "https://www.test.notify.nl/", DefaultValues.Models.EmptyUri.ToString()) },

                { "USER_AUTHORIZATION_JWT_SECRET",          GetTestValue(isValid, testString) },
                { "USER_AUTHORIZATION_JWT_ISSUER",          GetTestValue(isValid, testString) },
                { "USER_AUTHORIZATION_JWT_AUDIENCE",        GetTestValue(isValid, testString) },
                { "USER_AUTHORIZATION_JWT_EXPIRESINMIN",    GetTestValue(isValid, testUshort) },
                { "USER_AUTHORIZATION_JWT_USERID",          GetTestValue(isValid, testString) },
                { "USER_AUTHORIZATION_JWT_USERNAME",        GetTestValue(isValid, testString) },

                { "USER_API_KEY_OPENKLANT_2",               GetTestValue(isValid, testString) },
                { "USER_API_KEY_OBJECTEN",                  GetTestValue(isValid, testString) },
                { "USER_API_KEY_OBJECTTYPEN",               GetTestValue(isValid, testString) },
                { "USER_API_KEY_NOTIFYNL",                  GetTestValue(isValid, testString) },

                { "USER_DOMAIN_OPENNOTIFICATIES",           GetTestValue(isValid, testDomain) },
                { "USER_DOMAIN_OPENZAAK",                   GetTestValue(isValid, testDomain) },
                { "USER_DOMAIN_OPENKLANT",                  GetTestValue(isValid, testDomain, "http://domain") },
                { "USER_DOMAIN_OBJECTEN",                   GetTestValue(isValid, testDomain, "https://domain") },
                { "USER_DOMAIN_OBJECTTYPEN",                GetTestValue(isValid, testDomain, "domain/api/v1/typen") },

                { "USER_TEMPLATEIDS_EMAIL_ZAAKCREATE",      GetTestValue(isValid, testGuid) },
                { "USER_TEMPLATEIDS_EMAIL_ZAAKUPDATE",      GetTestValue(isValid, testGuid) },
                { "USER_TEMPLATEIDS_EMAIL_ZAAKCLOSE",       GetTestValue(isValid, testGuid) },
                { "USER_TEMPLATEIDS_EMAIL_TASKASSIGNED",    GetTestValue(isValid, testGuid) },
                { "USER_TEMPLATEIDS_EMAIL_MESSAGERECEIVED", GetTestValue(isValid, testGuid) },

                { "USER_TEMPLATEIDS_SMS_ZAAKCREATE",        GetTestValue(isValid, testGuid) },
                { "USER_TEMPLATEIDS_SMS_ZAAKUPDATE",        GetTestValue(isValid, testGuid) },
                { "USER_TEMPLATEIDS_SMS_ZAAKCLOSE",         GetTestValue(isValid, testGuid, "12345678-1234-12-34-1234-123456789012") },
                { "USER_TEMPLATEIDS_SMS_TASKASSIGNED",      GetTestValue(isValid, testGuid, "123456789-1234-1234-1234-123456789012") },
                { "USER_TEMPLATEIDS_SMS_MESSAGERECEIVED",   GetTestValue(isValid, testGuid, "!2345678-1234-12-34-1234-123456789*12") },

                { "USER_WHITELIST_ZAAKCREATE_IDS",          GetTestValue(isValid, testArray) },
                { "USER_WHITELIST_ZAAKUPDATE_IDS",          GetTestValue(isValid, testArray) },
                { "USER_WHITELIST_ZAAKCLOSE_IDS",           GetTestValue(isValid, testArray) },
                { "USER_WHITELIST_TASKASSIGNED_IDS",        GetTestValue(isValid, testArray) },
                { "USER_WHITELIST_DECISIONMADE_IDS",        GetTestValue(isValid, testArray) },
                { "USER_WHITELIST_MESSAGE_ALLOWED",         GetTestValue(isValid, testBool, "false") },  // NOTE: Could be also empty string, but "false" value is more useful for other tests
                { "USER_WHITELIST_TASKOBJECTTYPE_UUID",     GetTestValue(isValid, TestTaskObjectTypeUuid) },
                { "USER_WHITELIST_MESSAGEOBJECTTYPE_UUID",  GetTestValue(isValid, TestMessageObjectTypeUuid) }
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

        internal static WebApiConfiguration GetWebApiConfigurationWith(TestLoaderTypes testLoaderTypes)
            => s_presetConfigurations[testLoaderTypes];

        internal enum TestLoaderTypes
        {
            ValidAppSettings,
            InvalidAppSettings,
            ValidEnvironment,
            InvalidEnvironment,
            BothValid,
            BothInvalid
        }

        private static readonly Dictionary<TestLoaderTypes, WebApiConfiguration> s_presetConfigurations = new()
        {
            { TestLoaderTypes.ValidAppSettings,   GetWebApiConfiguration(TestLoaderTypes.ValidAppSettings)   },
            { TestLoaderTypes.InvalidAppSettings, GetWebApiConfiguration(TestLoaderTypes.InvalidAppSettings) },
            { TestLoaderTypes.ValidEnvironment,   GetWebApiConfiguration(TestLoaderTypes.ValidEnvironment)   },
            { TestLoaderTypes.InvalidEnvironment, GetWebApiConfiguration(TestLoaderTypes.InvalidEnvironment) },
            { TestLoaderTypes.BothValid,          GetWebApiConfiguration(TestLoaderTypes.BothValid)          },
            { TestLoaderTypes.BothInvalid,        GetWebApiConfiguration(TestLoaderTypes.BothInvalid)        }
        };

        private static WebApiConfiguration GetWebApiConfiguration(TestLoaderTypes loaderType)
        {
            // IServiceCollection
            var serviceCollection = new ServiceCollection();

            // ILoaderService
            switch (loaderType)
            {
                case TestLoaderTypes.ValidAppSettings:
                    serviceCollection.AddSingleton(GetAppSettingsLoader(isValid: true));
                    break;

                case TestLoaderTypes.InvalidAppSettings:
                    serviceCollection.AddSingleton(GetAppSettingsLoader(isValid: false));
                    break;

                case TestLoaderTypes.ValidEnvironment:
                    serviceCollection.AddSingleton(GetEnvironmentLoader(isValid: true));
                    break;

                case TestLoaderTypes.InvalidEnvironment:
                    serviceCollection.AddSingleton(GetEnvironmentLoader(isValid: false));
                    break;

                case TestLoaderTypes.BothValid:
                    serviceCollection.AddSingleton(GetAppSettingsLoader(isValid: true));
                    serviceCollection.AddSingleton(GetEnvironmentLoader(isValid: true));
                    break;

                default:
                case TestLoaderTypes.BothInvalid:
                    serviceCollection.AddSingleton(GetAppSettingsLoader(isValid: false));
                    serviceCollection.AddSingleton(GetEnvironmentLoader(isValid: false));
                    break;
            }

            // Remaining components of Web API Configuration
            return GetWebApiConfiguration(serviceCollection);
        }
        #endregion
    }
}