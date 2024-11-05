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
        internal const string TestTaskObjectTypeUuid    = "0236e468-2ad8-43d6-a723-219cb22acb37";
        internal const string TestMessageObjectTypeUuid = "9aae4a81-36c5-4fc9-958c-71ecdcdf48a7";
        internal const string TestInfoObjectTypeUuid1   = "38327774-7023-4f25-9386-acb0c6f10636";
        internal const string TestInfoObjectTypeUuid2   = "6468cfd4-d827-473a-8f24-114af046ce7f";

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
            const string testDomain = "test.domain/api/v1";

            // NOTE: Update the keys manually if the structure of the WebApiConfiguration change
            #region GetEnvironmentVariable<T>() mocking
            Dictionary<string /* Key */, string? /* Value */> keyValueMapping = new()
            {
                // OMC environment variables
                { "OMC_AUTH_JWT_SECRET",                         GetTestValue(isValid, testString) },
                { "OMC_AUTH_JWT_ISSUER",                         GetTestValue(isValid, testString) },
                { "OMC_AUTH_JWT_AUDIENCE",                       GetTestValue(isValid, testString) },
                { "OMC_AUTH_JWT_EXPIRESINMIN",                   GetTestValue(isValid, testUshort) },
                { "OMC_AUTH_JWT_USERID",                         GetTestValue(isValid, testString) },
                { "OMC_AUTH_JWT_USERNAME",                       GetTestValue(isValid, testString) },

                { "OMC_FEATURE_WORKFLOW_VERSION",                $"{omcWorkflow}" },

                // ZGW environment variables
                { "ZGW_AUTH_JWT_SECRET",                         GetTestValue(isValid, testString) },
                { "ZGW_AUTH_JWT_ISSUER",                         GetTestValue(isValid, testString) },
                { "ZGW_AUTH_JWT_AUDIENCE",                       GetTestValue(isValid, testString) },
                { "ZGW_AUTH_JWT_EXPIRESINMIN",                   GetTestValue(isValid, testUshort) },
                { "ZGW_AUTH_JWT_USERID",                         GetTestValue(isValid, testString) },
                { "ZGW_AUTH_JWT_USERNAME",                       GetTestValue(isValid, testString) },

                { "ZGW_AUTH_KEY_OPENKLANT",                      GetTestValue(isValid, testString) },
                { "ZGW_AUTH_KEY_OBJECTEN",                       GetTestValue(isValid, testString) },
                { "ZGW_AUTH_KEY_OBJECTTYPEN",                    GetTestValue(isValid, testString) },

                { "ZGW_ENDPOINT_OPENNOTIFICATIES",               GetTestValue(isValid, testDomain) },
                { "ZGW_ENDPOINT_OPENZAAK",                       GetTestValue(isValid, testDomain, " ") },
                { "ZGW_ENDPOINT_OPENKLANT",                      GetTestValue(isValid, testDomain, "http://domain") },
                { "ZGW_ENDPOINT_BESLUITEN",                      GetTestValue(isValid, testDomain) },
                { "ZGW_ENDPOINT_OBJECTEN",                       GetTestValue(isValid, testDomain, "https://domain") },
                { "ZGW_ENDPOINT_OBJECTTYPEN",                    GetTestValue(isValid, testDomain) },
                { "ZGW_ENDPOINT_CONTACTMOMENTEN",                GetTestValue(isValid, testDomain) },

                { "ZGW_WHITELIST_ZAAKCREATE_IDS",                GetTestValue(isValid, testArray) },
                { "ZGW_WHITELIST_ZAAKUPDATE_IDS",                GetTestValue(isValid, testArray) },
                { "ZGW_WHITELIST_ZAAKCLOSE_IDS",                 GetTestValue(isValid, "*") },  // NOTE: Everything is allowed
                { "ZGW_WHITELIST_TASKASSIGNED_IDS",              GetTestValue(isValid, testArray) },
                { "ZGW_WHITELIST_DECISIONMADE_IDS",              GetTestValue(isValid, testArray) },
                { "ZGW_WHITELIST_MESSAGE_ALLOWED",               GetTestValue(isValid, testBool, "false") },  // NOTE: Could be also empty string, but "false" value is more useful for other tests

                { "ZGW_VARIABLE_OBJECTEN_TASKOBJECTTYPE_UUID",          GetTestValue(isValid, TestTaskObjectTypeUuid) },
                { "ZGW_VARIABLE_OBJECTEN_MESSAGEOBJECTTYPE_UUID",       GetTestValue(isValid, TestMessageObjectTypeUuid) },
                { "ZGW_VARIABLE_OBJECTEN_MESSAGEOBJECTTYPE_VERSION",    GetTestValue(isValid, "1") },
                { "ZGW_VARIABLE_OBJECTEN_DECISIONINFOOBJECTTYPE_UUIDS", GetTestValue(isValid, $"{TestInfoObjectTypeUuid1}, {TestInfoObjectTypeUuid2}") },

                // NOTIFY environment variables
                { "NOTIFY_API_BASEURL",                          GetTestValue(isValid, "https://www.test.notify.nl/", DefaultValues.Models.EmptyUri.ToString()) },
                { "NOTIFY_API_KEY",                              GetTestValue(isValid, testString) },

                { "NOTIFY_TEMPLATEID_DECISIONMADE",             GetTestValue(isValid, testGuid) },

                { "NOTIFY_TEMPLATEID_EMAIL_ZAAKCREATE",         GetTestValue(isValid, testGuid) },
                { "NOTIFY_TEMPLATEID_EMAIL_ZAAKUPDATE",         GetTestValue(isValid, testGuid) },
                { "NOTIFY_TEMPLATEID_EMAIL_ZAAKCLOSE",          GetTestValue(isValid, testGuid) },
                { "NOTIFY_TEMPLATEID_EMAIL_TASKASSIGNED",       GetTestValue(isValid, testGuid) },
                { "NOTIFY_TEMPLATEID_EMAIL_MESSAGERECEIVED",    GetTestValue(isValid, testGuid) },

                { "NOTIFY_TEMPLATEID_SMS_ZAAKCREATE",           GetTestValue(isValid, testGuid) },
                { "NOTIFY_TEMPLATEID_SMS_ZAAKUPDATE",           GetTestValue(isValid, testGuid) },
                { "NOTIFY_TEMPLATEID_SMS_ZAAKCLOSE",            GetTestValue(isValid, testGuid, "12345678-1234-12-34-1234-123456789012") },
                { "NOTIFY_TEMPLATEID_SMS_TASKASSIGNED",         GetTestValue(isValid, testGuid, "123456789-1234-1234-1234-123456789012") },
                { "NOTIFY_TEMPLATEID_SMS_MESSAGERECEIVED",      GetTestValue(isValid, testGuid, "!2345678-1234-12-34-1234-123456789*12") }
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