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
        internal const string TestMessageObjectTypeUuid1 = "38327774-7023-4f25-9386-acb0c6f10636";
        internal const string TestMessageObjectTypeUuid2 = "6468cfd4-d827-473a-8f24-114af046ce7f";

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
        private static EnvironmentLoader GetEnvironmentLoader(byte omcWorkflow, bool isValid = true)
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

                { "OMC_FEATURES_WORKFLOW_VERSION",          $"{omcWorkflow}" },

                { "USER_AUTHORIZATION_JWT_SECRET",          GetTestValue(isValid, testString) },
                { "USER_AUTHORIZATION_JWT_ISSUER",          GetTestValue(isValid, testString) },
                { "USER_AUTHORIZATION_JWT_AUDIENCE",        GetTestValue(isValid, testString) },
                { "USER_AUTHORIZATION_JWT_EXPIRESINMIN",    GetTestValue(isValid, testUshort) },
                { "USER_AUTHORIZATION_JWT_USERID",          GetTestValue(isValid, testString) },
                { "USER_AUTHORIZATION_JWT_USERNAME",        GetTestValue(isValid, testString) },

                { "USER_API_KEY_OPENKLANT",                 GetTestValue(isValid, testString) },
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
                { "USER_WHITELIST_ZAAKCLOSE_IDS",           GetTestValue(isValid, "*") },  // NOTE: Everything is allowed
                { "USER_WHITELIST_TASKASSIGNED_IDS",        GetTestValue(isValid, testArray) },
                { "USER_WHITELIST_DECISIONMADE_IDS",        GetTestValue(isValid, testArray) },
                { "USER_WHITELIST_MESSAGE_ALLOWED",         GetTestValue(isValid, testBool, "false") },  // NOTE: Could be also empty string, but "false" value is more useful for other tests
                { "USER_WHITELIST_TASKOBJECTTYPE_UUID",     GetTestValue(isValid, TestTaskObjectTypeUuid) },
                { "USER_WHITELIST_MESSAGEOBJECTTYPE_UUIDS", GetTestValue(isValid, $"{TestMessageObjectTypeUuid1}, {TestMessageObjectTypeUuid2}") }
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
            // ReSharper disable InconsistentNaming
            ValidAppSettings,
            InvalidAppSettings,
            ValidEnvironment_v1,
            ValidEnvironment_v2,
            InvalidEnvironment,
            InvalidEnvironment_v1,
            InvalidEnvironment_v2,
            BothValid_v1,
            BothValid_v2,
            BothInvalid,
            BothInvalid_v1,
            BothInvalid_v2
        }

        private static readonly Dictionary<TestLoaderTypes, WebApiConfiguration> s_presetConfigurations = new()
        {
            { TestLoaderTypes.ValidAppSettings,      GetWebApiConfiguration(TestLoaderTypes.ValidAppSettings)      },
            { TestLoaderTypes.InvalidAppSettings,    GetWebApiConfiguration(TestLoaderTypes.InvalidAppSettings)    },
            { TestLoaderTypes.ValidEnvironment_v1,   GetWebApiConfiguration(TestLoaderTypes.ValidEnvironment_v1)   },
            { TestLoaderTypes.ValidEnvironment_v2,   GetWebApiConfiguration(TestLoaderTypes.ValidEnvironment_v2)   },
            { TestLoaderTypes.InvalidEnvironment,    GetWebApiConfiguration(TestLoaderTypes.InvalidEnvironment)    },
            { TestLoaderTypes.InvalidEnvironment_v1, GetWebApiConfiguration(TestLoaderTypes.InvalidEnvironment_v1) },
            { TestLoaderTypes.InvalidEnvironment_v2, GetWebApiConfiguration(TestLoaderTypes.InvalidEnvironment_v2) },
            { TestLoaderTypes.BothValid_v1,          GetWebApiConfiguration(TestLoaderTypes.BothValid_v1)          },
            { TestLoaderTypes.BothValid_v2,          GetWebApiConfiguration(TestLoaderTypes.BothValid_v2)          },
            { TestLoaderTypes.BothInvalid,           GetWebApiConfiguration(TestLoaderTypes.BothInvalid)           },
            { TestLoaderTypes.BothInvalid_v1,        GetWebApiConfiguration(TestLoaderTypes.BothInvalid_v1)        },
            { TestLoaderTypes.BothInvalid_v2,        GetWebApiConfiguration(TestLoaderTypes.BothInvalid_v2)        }
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

                case TestLoaderTypes.ValidEnvironment_v1:
                    serviceCollection.AddSingleton(GetEnvironmentLoader(1, isValid: true));
                    break;

                case TestLoaderTypes.ValidEnvironment_v2:
                    serviceCollection.AddSingleton(GetEnvironmentLoader(2, isValid: true));
                    break;

                case TestLoaderTypes.InvalidEnvironment:
                    serviceCollection.AddSingleton(GetEnvironmentLoader(0, isValid: false));
                    break;

                case TestLoaderTypes.InvalidEnvironment_v1:
                    serviceCollection.AddSingleton(GetEnvironmentLoader(1, isValid: false));
                    break;

                case TestLoaderTypes.InvalidEnvironment_v2:
                    serviceCollection.AddSingleton(GetEnvironmentLoader(2, isValid: false));
                    break;

                case TestLoaderTypes.BothValid_v1:
                    serviceCollection.AddSingleton(GetAppSettingsLoader(isValid: true));
                    serviceCollection.AddSingleton(GetEnvironmentLoader(1, isValid: true));
                    break;

                case TestLoaderTypes.BothValid_v2:
                    serviceCollection.AddSingleton(GetAppSettingsLoader(isValid: true));
                    serviceCollection.AddSingleton(GetEnvironmentLoader(2, isValid: true));
                    break;

                default:
                case TestLoaderTypes.BothInvalid:
                    serviceCollection.AddSingleton(GetAppSettingsLoader(isValid: false));
                    serviceCollection.AddSingleton(GetEnvironmentLoader(0, isValid: false));
                    break;

                case TestLoaderTypes.BothInvalid_v1:
                    serviceCollection.AddSingleton(GetAppSettingsLoader(isValid: false));
                    serviceCollection.AddSingleton(GetEnvironmentLoader(1, isValid: false));
                    break;

                case TestLoaderTypes.BothInvalid_v2:
                    serviceCollection.AddSingleton(GetAppSettingsLoader(isValid: false));
                    serviceCollection.AddSingleton(GetEnvironmentLoader(2, isValid: false));
                    break;
            }

            // Remaining components of Web API Configuration
            return GetWebApiConfiguration(serviceCollection);
        }
        #endregion
    }
}