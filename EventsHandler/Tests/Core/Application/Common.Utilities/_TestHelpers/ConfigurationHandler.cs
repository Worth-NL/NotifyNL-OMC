// © 2023, Worth Systems.

using Common.Constants;
using Common.Extensions;
using Common.Settings;
using Common.Settings.Configuration;
using Common.Settings.DAO.Interfaces;
using Common.Settings.Enums;
using Common.Settings.Interfaces;
using Common.Settings.Strategy.Interfaces;
using Common.Settings.Strategy.Manager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MoqExt;

namespace Common.Tests.Utilities._TestHelpers
{
    /// <summary>
    /// The configuration handler used for test project.
    /// </summary>
    public static class ConfigurationHandler
    {
        // NOTE: IConfiguration

        #region IConfiguration
        /// <summary>
        /// Gets the test <see cref="IConfiguration"/> with (re)created "appsettings.Test.json" and environment variables.
        /// </summary>
        public static IConfiguration GetConfiguration()
        {
            const string appSettingsFileName = "appsettings.Test.json";

            string appSettingsPath = Path.Combine(AppContext.BaseDirectory, appSettingsFileName);

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile(appSettingsPath)
                .Build();

            return configuration;
        }
        #endregion

        // NOTE: IConfiguration or IEnvironment => ILoadersContext

        #region ILoadingService

        public const string TestTaskObjectTypeUuid    = "0236e468-2ad8-43d6-a723-219cb22acb37";
        public const string TestMessageObjectTypeUuid = "9aae4a81-36c5-4fc9-958c-71ecdcdf48a7";
        public const string TestInfoObjectTypeUuid1   = "38327774-7023-4f25-9386-acb0c6f10636";
        public const string TestInfoObjectTypeUuid2   = "6468cfd4-d827-473a-8f24-114af046ce7f";

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
            const string testArray  = "1, 2, 3";
            const string testUshort = "60";
            const string testGuid   = "01234567-89ab-cdef-1234-567890123456";
            const string testBool   = "true";
            const string testDomain = "test.domain/api/v1";

            // NOTE: Update the keys manually if the structure of the WebApiConfiguration change
            #region GetEnvironmentVariable<T>() mocking
            Dictionary<string /* Key */, string? /* Value */> keyValueMapping = new()
            {
                // OMC environment variables
                { "OMC_AUTH_JWT_SECRET",                                  GetTestValue(isValid, testString) },
                { "OMC_AUTH_JWT_ISSUER",                                  GetTestValue(isValid, testString) },
                { "OMC_AUTH_JWT_AUDIENCE",                                GetTestValue(isValid, testString) },
                { "OMC_AUTH_JWT_EXPIRESINMIN",                            GetTestValue(isValid, testUshort) },
                { "OMC_AUTH_JWT_USERID",                                  GetTestValue(isValid, testString) },
                { "OMC_AUTH_JWT_USERNAME",                                GetTestValue(isValid, testString) },

                { "OMC_FEATURE_WORKFLOW_VERSION",                         $"{omcWorkflow}" },

                // ZGW environment variables
                { "ZGW_AUTH_JWT_SECRET",                                  GetTestValue(isValid, testString) },
                { "ZGW_AUTH_JWT_ISSUER",                                  GetTestValue(isValid, testString) },
                { "ZGW_AUTH_JWT_AUDIENCE",                                GetTestValue(isValid, testString) },
                { "ZGW_AUTH_JWT_EXPIRESINMIN",                            GetTestValue(isValid, testUshort) },
                { "ZGW_AUTH_JWT_USERID",                                  GetTestValue(isValid, testString) },
                { "ZGW_AUTH_JWT_USERNAME",                                GetTestValue(isValid, testString) },

                { "ZGW_AUTH_KEY_OPENKLANT",                               GetTestValue(isValid, testString) },
                { "ZGW_AUTH_KEY_OBJECTEN",                                GetTestValue(isValid, testString) },
                { "ZGW_AUTH_KEY_OBJECTTYPEN",                             GetTestValue(isValid, testString) },

                { "ZGW_ENDPOINT_OPENNOTIFICATIES",                        GetTestValue(isValid, testDomain) },
                { "ZGW_ENDPOINT_OPENZAAK",                                GetTestValue(isValid, testDomain, " ") },
                { "ZGW_ENDPOINT_OPENKLANT",                               GetTestValue(isValid, testDomain, "http://domain") },
                { "ZGW_ENDPOINT_BESLUITEN",                               GetTestValue(isValid, testDomain) },
                { "ZGW_ENDPOINT_OBJECTEN",                                GetTestValue(isValid, testDomain, "https://domain") },
                { "ZGW_ENDPOINT_OBJECTTYPEN",                             GetTestValue(isValid, testDomain) },
                { "ZGW_ENDPOINT_CONTACTMOMENTEN",                         GetTestValue(isValid, testDomain) },

                { "ZGW_WHITELIST_ZAAKCREATE_IDS",                         GetTestValue(isValid, testArray) },
                { "ZGW_WHITELIST_ZAAKUPDATE_IDS",                         GetTestValue(isValid, testArray) },
                { "ZGW_WHITELIST_ZAAKCLOSE_IDS",                          GetTestValue(isValid, "*") },  // NOTE: Everything is allowed
                { "ZGW_WHITELIST_TASKASSIGNED_IDS",                       GetTestValue(isValid, testArray) },
                { "ZGW_WHITELIST_DECISIONMADE_IDS",                       GetTestValue(isValid, testArray) },
                { "ZGW_WHITELIST_MESSAGE_ALLOWED",                        GetTestValue(isValid, testBool, "false") },  // NOTE: Could be also empty string, but "false" value is more useful for other tests

                { "ZGW_VARIABLE_OBJECTTYPE_TASKOBJECTTYPE_UUID",          GetTestValue(isValid, TestTaskObjectTypeUuid) },
                { "ZGW_VARIABLE_OBJECTTYPE_MESSAGEOBJECTTYPE_UUID",       GetTestValue(isValid, TestMessageObjectTypeUuid) },
                { "ZGW_VARIABLE_OBJECTTYPE_MESSAGEOBJECTTYPE_VERSION",    GetTestValue(isValid, "1") },
                { "ZGW_VARIABLE_OBJECTTYPE_DECISIONINFOOBJECTTYPE_UUIDS", GetTestValue(isValid, $"{TestInfoObjectTypeUuid1}, {TestInfoObjectTypeUuid2}") },

                // NOTIFY environment variables
                { "NOTIFY_API_BASEURL",                                   GetTestValue(isValid, "https://www.test.notify.nl/", DefaultValues.Models.EmptyUri.ToString()) },
                { "NOTIFY_API_KEY",                                       GetTestValue(isValid, testString) },

                { "NOTIFY_TEMPLATEID_DECISIONMADE",                       GetTestValue(isValid, testGuid) },

                { "NOTIFY_TEMPLATEID_EMAIL_ZAAKCREATE",                   GetTestValue(isValid, testGuid) },
                { "NOTIFY_TEMPLATEID_EMAIL_ZAAKUPDATE",                   GetTestValue(isValid, testGuid) },
                { "NOTIFY_TEMPLATEID_EMAIL_ZAAKCLOSE",                    GetTestValue(isValid, testGuid) },
                { "NOTIFY_TEMPLATEID_EMAIL_TASKASSIGNED",                 GetTestValue(isValid, testGuid) },
                { "NOTIFY_TEMPLATEID_EMAIL_MESSAGERECEIVED",              GetTestValue(isValid, testGuid) },

                { "NOTIFY_TEMPLATEID_SMS_ZAAKCREATE",                     GetTestValue(isValid, testGuid) },
                { "NOTIFY_TEMPLATEID_SMS_ZAAKUPDATE",                     GetTestValue(isValid, testGuid) },
                { "NOTIFY_TEMPLATEID_SMS_ZAAKCLOSE",                      GetTestValue(isValid, testGuid, "12345678-1234-12-34-1234-123456789012") },
                { "NOTIFY_TEMPLATEID_SMS_TASKASSIGNED",                   GetTestValue(isValid, testGuid, "123456789-1234-1234-1234-123456789012") },
                { "NOTIFY_TEMPLATEID_SMS_MESSAGERECEIVED",                GetTestValue(isValid, testGuid, "!2345678-1234-12-34-1234-123456789*12") }
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

        public const string EnvPrefix = "Env_";

        /// <summary>
        /// Gets the mocked <see cref="EnvironmentLoader"/> overloading configurations in "appsettings.json" from <see cref="AppSettingsLoader"/>.
        /// </summary>
        private static EnvironmentLoader GetEnvVarsOverloadingAppSettings()
        {
            var mockedEnvironmentReader = new Mock<IEnvironment>();

            #region GetEnvironmentVariable<T>() mocking
            // Get all final paths (without nested elements) with values from "appsettings.json"
            (string Path, string Value)[] sections = GetConfiguration().GetChildren()
                .SelectMany(GetAllPaths)
                // Convert paths "app:settings:item" into "ENVIRONMENT_VARIABLE" convention
                .Select(section => (
                    // Path
                    section.Path
                        .ToUpper()
                        .Replace(":", "_")
                        .Replace(".", "_"),
                    // Value
                    section.Value))
                .ToArray();

            foreach ((string Path, string Value) section in sections)
            {
                mockedEnvironmentReader
                    .Setup(mock => mock.GetEnvironmentVariable(section.Path))
                    .Returns($"{EnvPrefix}{section.Value}");
            }
            #endregion

            return new EnvironmentLoader
            {
                Environment = mockedEnvironmentReader.Object
            };

            static IEnumerable<(string Path, string Value)> GetAllPaths(IConfigurationSection section)
            {
                IConfigurationSection[] configurationSections = section.GetChildren().ToArray();

                // Dive deeper into nested levels of a given section
                if (configurationSections.HasAny())
                {
                    foreach (IConfigurationSection configurationSection in configurationSections)
                    {
                        foreach ((string Path, string Value) item in GetAllPaths(configurationSection))
                        {
                            yield return (item.Path, item.Value);  // NOTE: This line is returning the results (row by row) of
                                                                   // GetAllPaths method, where empty values are skipped already
                        }
                    }
                }

                // Return section which doesn't have more nested levels
                if (section.Value.IsNullOrEmpty())
                {
                    yield break;  // Skip path with empty value because that's most likely a "parent node"
                }

                yield return (section.Path, section.Value);
            }
        }
        #endregion

        // NOTE: IServiceCollection[] { ILoadersContext } => IServiceProvider

        #region ILoadersContext

        public static ILoadersContext GetLoadersContext(LoaderTypes loaderTypes)
        {
            // IServiceCollection
            ServiceCollection serviceCollection = [];

            serviceCollection.AddSingleton<ILoadingService>(loaderTypes switch
            {
                LoaderTypes.AppSettings => GetAppSettingsLoader(),
                LoaderTypes.Environment => GetEnvironmentLoader(1),

                _ => throw new ArgumentException($"Not supported loader type: {loaderTypes}")
            });

            // ILoadersContext
            ILoadersContext loadersContext = new LoadersContext(
                // IServiceProvider
                GetServiceProvider(serviceCollection));

            loadersContext.SetLoader(loaderTypes);

            return loadersContext;
        }

        private static MockingContext GetServiceProvider(ServiceCollection serviceCollection)
        {
            // IServiceProvider
            return new MockingContext(serviceCollection);
        }
        #endregion

        // NOTE: IServiceProvider => WebApiConfiguration

        #region Web API Configuration
        public static WebApiConfiguration GetWebApiConfiguration()
        {
            // Web API Configuration
            return GetWebApiConfiguration([]);
        }

        private static WebApiConfiguration GetWebApiConfiguration(ServiceCollection serviceCollection)
        {
            // Web API Configuration
            return new WebApiConfiguration(GetServiceProvider(serviceCollection));
        }

        public static WebApiConfiguration GetWebApiConfigurationWith(TestLoaderTypesSetup testLoaderTypes)
            => s_presetConfigurations[testLoaderTypes];

        /// <summary>
        /// The enum representing <see cref="LoaderTypes"/> in preconfigured setups used for testing.
        /// </summary>
        public enum TestLoaderTypesSetup
        {
            // ReSharper disable InconsistentNaming

            /// <summary>
            /// Valid appsettings.json | not existing environment variables.
            /// </summary>
            ValidAppSettings,

            /// <summary>
            /// Invalid appsettings.json | not existing environment variables.
            /// </summary>
            InvalidAppSettings,

            /// <summary>
            /// Not existing appsettings.json | valid environment variables (OMC workflow v1).
            /// </summary>
            ValidEnvironment_v1,

            /// <summary>
            /// Not existing appsettings.json | valid environment variables (OMC workflow v2).
            /// </summary>
            ValidEnvironment_v2,

            /// <summary>
            /// Not existing appsettings.json | invalid environment variables (+ wrong OMC workflow).
            /// </summary>
            InvalidEnvironment,

            /// <summary>
            /// Not existing appsettings.json | invalid environment variables (OMC workflow v1).
            /// </summary>
            InvalidEnvironment_v1,

            /// <summary>
            /// Not existing appsettings.json | invalid environment variables (OMC workflow v2).
            /// </summary>
            InvalidEnvironment_v2,

            /// <summary>
            /// Valid appsettings.json | valid environment variables (OMC workflow v1).
            /// </summary>
            BothValid_v1,

            /// <summary>
            /// Valid appsettings.json | valid environment variables (OMC workflow v2).
            /// </summary>
            BothValid_v2,

            /// <summary>
            /// Invalid appsettings.json | invalid environment variables (+ wrong OMC workflow).
            /// </summary>
            BothInvalid,

            /// <summary>
            /// Invalid appsettings.json | invalid environment variables (OMC workflow v1).
            /// </summary>
            BothInvalid_v1,

            /// <summary>
            /// Invalid appsettings.json | invalid environment variables (OMC workflow v2).
            /// </summary>
            BothInvalid_v2,

            /// <summary>
            /// Environment variables using "underscore" convention reflecting appsettings.json | valid appsettings.json (overloaded by environment variables).
            /// </summary>
            EnvVar_Overloading_AppSettings
        }

        private static readonly Dictionary<TestLoaderTypesSetup, WebApiConfiguration> s_presetConfigurations = new()
        {
            { TestLoaderTypesSetup.ValidAppSettings,               GetWebApiConfiguration(TestLoaderTypesSetup.ValidAppSettings)               },
            { TestLoaderTypesSetup.InvalidAppSettings,             GetWebApiConfiguration(TestLoaderTypesSetup.InvalidAppSettings)             },
            { TestLoaderTypesSetup.ValidEnvironment_v1,            GetWebApiConfiguration(TestLoaderTypesSetup.ValidEnvironment_v1)            },
            { TestLoaderTypesSetup.ValidEnvironment_v2,            GetWebApiConfiguration(TestLoaderTypesSetup.ValidEnvironment_v2)            },
            { TestLoaderTypesSetup.InvalidEnvironment,             GetWebApiConfiguration(TestLoaderTypesSetup.InvalidEnvironment)             },
            { TestLoaderTypesSetup.InvalidEnvironment_v1,          GetWebApiConfiguration(TestLoaderTypesSetup.InvalidEnvironment_v1)          },
            { TestLoaderTypesSetup.InvalidEnvironment_v2,          GetWebApiConfiguration(TestLoaderTypesSetup.InvalidEnvironment_v2)          },
            { TestLoaderTypesSetup.BothValid_v1,                   GetWebApiConfiguration(TestLoaderTypesSetup.BothValid_v1)                   },
            { TestLoaderTypesSetup.BothValid_v2,                   GetWebApiConfiguration(TestLoaderTypesSetup.BothValid_v2)                   },
            { TestLoaderTypesSetup.BothInvalid,                    GetWebApiConfiguration(TestLoaderTypesSetup.BothInvalid)                    },
            { TestLoaderTypesSetup.BothInvalid_v1,                 GetWebApiConfiguration(TestLoaderTypesSetup.BothInvalid_v1)                 },
            { TestLoaderTypesSetup.BothInvalid_v2,                 GetWebApiConfiguration(TestLoaderTypesSetup.BothInvalid_v2)                 },
            { TestLoaderTypesSetup.EnvVar_Overloading_AppSettings, GetWebApiConfiguration(TestLoaderTypesSetup.EnvVar_Overloading_AppSettings) }
        };

        private static WebApiConfiguration GetWebApiConfiguration(TestLoaderTypesSetup loaderType)
        {
            // IServiceCollection
            var serviceCollection = new ServiceCollection();

            // ILoaderService
            switch (loaderType)
            {
                case TestLoaderTypesSetup.ValidAppSettings:
                    serviceCollection.AddSingleton(GetAppSettingsLoader(isValid: true));
                    // Not existing environment variables
                    break;

                case TestLoaderTypesSetup.InvalidAppSettings:
                    serviceCollection.AddSingleton(GetAppSettingsLoader(isValid: false));
                    // Not existing environment variables
                    break;

                case TestLoaderTypesSetup.ValidEnvironment_v1:
                    // Not existing appsettings.json
                    serviceCollection.AddSingleton(GetEnvironmentLoader(1, isValid: true));
                    break;

                case TestLoaderTypesSetup.ValidEnvironment_v2:
                    // Not existing appsettings.json
                    serviceCollection.AddSingleton(GetEnvironmentLoader(2, isValid: true));
                    break;

                case TestLoaderTypesSetup.InvalidEnvironment:
                    // Not existing appsettings.json
                    serviceCollection.AddSingleton(GetEnvironmentLoader(0, isValid: false));
                    break;

                case TestLoaderTypesSetup.InvalidEnvironment_v1:
                    // Not existing appsettings.json
                    serviceCollection.AddSingleton(GetEnvironmentLoader(1, isValid: false));
                    break;

                case TestLoaderTypesSetup.InvalidEnvironment_v2:
                    // Not existing appsettings.json
                    serviceCollection.AddSingleton(GetEnvironmentLoader(2, isValid: false));
                    break;

                case TestLoaderTypesSetup.BothValid_v1:
                    serviceCollection.AddSingleton(GetAppSettingsLoader(isValid: true));
                    serviceCollection.AddSingleton(GetEnvironmentLoader(1, isValid: true));
                    break;

                case TestLoaderTypesSetup.BothValid_v2:
                    serviceCollection.AddSingleton(GetAppSettingsLoader(isValid: true));
                    serviceCollection.AddSingleton(GetEnvironmentLoader(2, isValid: true));
                    break;

                default:
                case TestLoaderTypesSetup.BothInvalid:
                    serviceCollection.AddSingleton(GetAppSettingsLoader(isValid: false));
                    serviceCollection.AddSingleton(GetEnvironmentLoader(0, isValid: false));
                    break;

                case TestLoaderTypesSetup.BothInvalid_v1:
                    serviceCollection.AddSingleton(GetAppSettingsLoader(isValid: false));
                    serviceCollection.AddSingleton(GetEnvironmentLoader(1, isValid: false));
                    break;

                case TestLoaderTypesSetup.BothInvalid_v2:
                    serviceCollection.AddSingleton(GetAppSettingsLoader(isValid: false));
                    serviceCollection.AddSingleton(GetEnvironmentLoader(2, isValid: false));
                    break;

                case TestLoaderTypesSetup.EnvVar_Overloading_AppSettings:
                    serviceCollection.AddSingleton(GetAppSettingsLoader(isValid: true));  // NOTE: AppSettings are absolutely ok and valid here, as one of the loading strategies
                    serviceCollection.AddSingleton(GetEnvVarsOverloadingAppSettings());   // NOTE: But in "FallbackContextWrapper" the EnvironmentVariables will take the priority
                    break;
            }

            // Remaining components of Web API Configuration
            return GetWebApiConfiguration(serviceCollection);
        }
        #endregion
    }
}