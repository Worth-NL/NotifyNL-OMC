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
            const string testValues = "1, 2, 3";
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
                { "OMC_AUTHORIZATION_JWT_SECRET",        GetTestValue(isValid, testValue) },
                { "OMC_AUTHORIZATION_JWT_ISSUER",        GetTestValue(isValid, testValue) },
                { "OMC_AUTHORIZATION_JWT_AUDIENCE",      GetTestValue(isValid, testValue) },
                { "OMC_AUTHORIZATION_JWT_USERID",        GetTestValue(isValid, testValue) },
                { "OMC_AUTHORIZATION_JWT_USERNAME",      GetTestValue(isValid, testValue) },
                                                         
                { "OMC_API_BASEURL_NOTIFYNL",            GetTestValue(isValid, "https://www.test.nl/") },
                                                         
                { "USER_AUTHORIZATION_JWT_SECRET",       GetTestValue(isValid, testValue) },
                { "USER_AUTHORIZATION_JWT_ISSUER",       GetTestValue(isValid, testValue) },
                { "USER_AUTHORIZATION_JWT_AUDIENCE",     GetTestValue(isValid, testValue) },
                { "USER_AUTHORIZATION_JWT_USERID",       GetTestValue(isValid, testValue) },
                { "USER_AUTHORIZATION_JWT_USERNAME",     GetTestValue(isValid, testValue) },
                                                         
                { "USER_API_KEY_OPENKLANT_2",            GetTestValue(isValid, testValue) },
                { "USER_API_KEY_OBJECTEN",               GetTestValue(isValid, testValue) },
                { "USER_API_KEY_OBJECTTYPEN",            GetTestValue(isValid, testValue) },
                { "USER_API_KEY_NOTIFYNL",               GetTestValue(isValid, testValue) },
                                                         
                { "USER_DOMAIN_OPENNOTIFICATIES",        GetTestValue(isValid, testDomain) },
                { "USER_DOMAIN_OPENZAAK",                GetTestValue(isValid, testDomain) },
                { "USER_DOMAIN_OPENKLANT",               GetTestValue(isValid, testDomain, "http://domain") },
                { "USER_DOMAIN_OBJECTEN",                GetTestValue(isValid, testDomain, "https://domain") },
                { "USER_DOMAIN_OBJECTTYPEN",             GetTestValue(isValid, testDomain, "domain/api/v1/typen") },

                { "USER_TEMPLATEIDS_EMAIL_ZAAKCREATE",   GetTestValue(isValid, testTempId) },
                { "USER_TEMPLATEIDS_EMAIL_ZAAKUPDATE",   GetTestValue(isValid, testTempId) },
                { "USER_TEMPLATEIDS_EMAIL_ZAAKCLOSE",    GetTestValue(isValid, testTempId) },
                { "USER_TEMPLATEIDS_EMAIL_TASKASSIGNED", GetTestValue(isValid, testTempId) },
                { "USER_TEMPLATEIDS_EMAIL_DECISIONMADE", GetTestValue(isValid, testTempId) },

                { "USER_TEMPLATEIDS_SMS_ZAAKCREATE",     GetTestValue(isValid, testTempId) },
                { "USER_TEMPLATEIDS_SMS_ZAAKUPDATE",     GetTestValue(isValid, testTempId) },
                { "USER_TEMPLATEIDS_SMS_ZAAKCLOSE",      GetTestValue(isValid, testTempId, "12345678-1234-12-34-1234-123456789012") },
                { "USER_TEMPLATEIDS_SMS_TASKASSIGNED",   GetTestValue(isValid, testTempId, "123456789-1234-1234-1234-123456789012") },
                { "USER_TEMPLATEIDS_SMS_DECISIONMADE",   GetTestValue(isValid, testTempId, "!2345678-1234-12-34-1234-123456789*12") },

                { "USER_WHITELIST_ZAAKCREATE_IDS",       GetTestValue(isValid, testValues) },
                { "USER_WHITELIST_ZAAKUPDATE_IDS",       GetTestValue(isValid, testValues) },
                { "USER_WHITELIST_ZAAKCLOSE_IDS",        GetTestValue(isValid, testValues) },
                { "USER_WHITELIST_TASKASSIGNED_IDS",     GetTestValue(isValid, testValues) },
                { "USER_WHITELIST_DECISIONMADE_IDS",     GetTestValue(isValid, testValues) },
                { "USER_WHITELIST_MESSAGE_ALLOWED",      GetTestValue(isValid, "false") }
            };

            foreach (KeyValuePair<string, string> keyValue in keyValueMapping)
            {
                mockedEnvironmentLoader
                    .Setup(mock => mock.GetData<string>(keyValue.Key))
                    .Returns(keyValue.Value);
            }

            // NOTE: These environment variables are the numeric ones
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
                ($"{nameof(WebApiConfiguration.OMC)}",  $"{nameof(WebApiConfiguration.OMC.Authorization)}", "OMC_AUTHORIZATION"),
                ($"{nameof(WebApiConfiguration.User)}", $"{nameof(WebApiConfiguration.OMC.Authorization)}", "USER_AUTHORIZATION"),

                ("OMC_AUTHORIZATION",     $"{nameof(WebApiConfiguration.OMC.Authorization.JWT)}",              "OMC_AUTHORIZATION_JWT"),
                ("OMC_AUTHORIZATION_JWT", $"{nameof(WebApiConfiguration.OMC.Authorization.JWT.Secret)}",       "OMC_AUTHORIZATION_JWT_SECRET"),
                ("OMC_AUTHORIZATION_JWT", $"{nameof(WebApiConfiguration.OMC.Authorization.JWT.Issuer)}",       "OMC_AUTHORIZATION_JWT_ISSUER"),
                ("OMC_AUTHORIZATION_JWT", $"{nameof(WebApiConfiguration.OMC.Authorization.JWT.Audience)}",     "OMC_AUTHORIZATION_JWT_AUDIENCE"),
                ("OMC_AUTHORIZATION_JWT", $"{nameof(WebApiConfiguration.OMC.Authorization.JWT.ExpiresInMin)}", "OMC_AUTHORIZATION_JWT_EXPIRESINMIN"),
                ("OMC_AUTHORIZATION_JWT", $"{nameof(WebApiConfiguration.OMC.Authorization.JWT.UserId)}",       "OMC_AUTHORIZATION_JWT_USERID"),
                ("OMC_AUTHORIZATION_JWT", $"{nameof(WebApiConfiguration.OMC.Authorization.JWT.UserName)}",     "OMC_AUTHORIZATION_JWT_USERNAME"),

                ("OMC",             $"{nameof(WebApiConfiguration.OMC.API)}",                  "OMC_API"),
                ("OMC_API",         $"{nameof(WebApiConfiguration.OMC.API.BaseUrl)}",          "OMC_API_BASEURL"),
                ("OMC_API_BASEURL", $"{nameof(WebApiConfiguration.OMC.API.BaseUrl.NotifyNL)}", "OMC_API_BASEURL_NOTIFYNL"),

                ("USER_AUTHORIZATION",     $"{nameof(WebApiConfiguration.User.Authorization.JWT)}",              "USER_AUTHORIZATION_JWT"),
                ("USER_AUTHORIZATION_JWT", $"{nameof(WebApiConfiguration.User.Authorization.JWT.Secret)}",       "USER_AUTHORIZATION_JWT_SECRET"),
                ("USER_AUTHORIZATION_JWT", $"{nameof(WebApiConfiguration.User.Authorization.JWT.Issuer)}",       "USER_AUTHORIZATION_JWT_ISSUER"),
                ("USER_AUTHORIZATION_JWT", $"{nameof(WebApiConfiguration.User.Authorization.JWT.Audience)}",     "USER_AUTHORIZATION_JWT_AUDIENCE"),
                ("USER_AUTHORIZATION_JWT", $"{nameof(WebApiConfiguration.User.Authorization.JWT.ExpiresInMin)}", "USER_AUTHORIZATION_JWT_EXPIRESINMIN"),
                ("USER_AUTHORIZATION_JWT", $"{nameof(WebApiConfiguration.User.Authorization.JWT.UserId)}",       "USER_AUTHORIZATION_JWT_USERID"),
                ("USER_AUTHORIZATION_JWT", $"{nameof(WebApiConfiguration.User.Authorization.JWT.UserName)}",     "USER_AUTHORIZATION_JWT_USERNAME"),

                ("User",         $"{nameof(WebApiConfiguration.User.API)}",                 "USER_API"),
                ("USER_API",     $"{nameof(WebApiConfiguration.User.API.Key)}",             "USER_API_KEY"),
                ("USER_API_KEY", $"{nameof(WebApiConfiguration.User.API.Key.OpenKlant_2)}", "USER_API_KEY_OPENKLANT_2"),
                ("USER_API_KEY", $"{nameof(WebApiConfiguration.User.API.Key.Objecten)}",    "USER_API_KEY_OBJECTEN"),
                ("USER_API_KEY", $"{nameof(WebApiConfiguration.User.API.Key.ObjectTypen)}", "USER_API_KEY_OBJECTTYPEN"),
                ("USER_API_KEY", $"{nameof(WebApiConfiguration.User.API.Key.NotifyNL)}",    "USER_API_KEY_NOTIFYNL"),

                ("User",        $"{nameof(WebApiConfiguration.User.Domain)}",                  "USER_DOMAIN"),
                ("USER_DOMAIN", $"{nameof(WebApiConfiguration.User.Domain.OpenNotificaties)}", "USER_DOMAIN_OPENNOTIFICATIES"),
                ("USER_DOMAIN", $"{nameof(WebApiConfiguration.User.Domain.OpenZaak)}",         "USER_DOMAIN_OPENZAAK"),
                ("USER_DOMAIN", $"{nameof(WebApiConfiguration.User.Domain.OpenKlant)}",        "USER_DOMAIN_OPENKLANT"),
                ("USER_DOMAIN", $"{nameof(WebApiConfiguration.User.Domain.Objecten)}",         "USER_DOMAIN_OBJECTEN"),
                ("USER_DOMAIN", $"{nameof(WebApiConfiguration.User.Domain.ObjectTypen)}",      "USER_DOMAIN_OBJECTTYPEN"),

                ("User",                   $"{nameof(WebApiConfiguration.User.TemplateIds)}",                    "USER_TEMPLATEIDS"),
                ("USER_TEMPLATEIDS",       $"{nameof(WebApiConfiguration.User.TemplateIds.Email)}",              "USER_TEMPLATEIDS_EMAIL"),
                ("USER_TEMPLATEIDS_EMAIL", $"{nameof(WebApiConfiguration.User.TemplateIds.Email.ZaakCreate)}",   "USER_TEMPLATEIDS_EMAIL_ZAAKCREATE"),
                ("USER_TEMPLATEIDS_EMAIL", $"{nameof(WebApiConfiguration.User.TemplateIds.Email.ZaakUpdate)}",   "USER_TEMPLATEIDS_EMAIL_ZAAKUPDATE"),
                ("USER_TEMPLATEIDS_EMAIL", $"{nameof(WebApiConfiguration.User.TemplateIds.Email.ZaakClose)}",    "USER_TEMPLATEIDS_EMAIL_ZAAKCLOSE"),
                ("USER_TEMPLATEIDS_EMAIL", $"{nameof(WebApiConfiguration.User.TemplateIds.Email.TaskAssigned)}", "USER_TEMPLATEIDS_EMAIL_TASKASSIGNED"),
                ("USER_TEMPLATEIDS_EMAIL", $"{nameof(WebApiConfiguration.User.TemplateIds.Email.DecisionMade)}", "USER_TEMPLATEIDS_EMAIL_DECISIONMADE"),

                ("USER_TEMPLATEIDS",     $"{nameof(WebApiConfiguration.User.TemplateIds.Sms)}",              "USER_TEMPLATEIDS_SMS"),
                ("USER_TEMPLATEIDS_SMS", $"{nameof(WebApiConfiguration.User.TemplateIds.Sms.ZaakCreate)}",   "USER_TEMPLATEIDS_SMS_ZAAKCREATE"),
                ("USER_TEMPLATEIDS_SMS", $"{nameof(WebApiConfiguration.User.TemplateIds.Sms.ZaakUpdate)}",   "USER_TEMPLATEIDS_SMS_ZAAKUPDATE"),
                ("USER_TEMPLATEIDS_SMS", $"{nameof(WebApiConfiguration.User.TemplateIds.Sms.ZaakClose)}",    "USER_TEMPLATEIDS_SMS_ZAAKCLOSE"),
                ("USER_TEMPLATEIDS_SMS", $"{nameof(WebApiConfiguration.User.TemplateIds.Sms.TaskAssigned)}", "USER_TEMPLATEIDS_SMS_TASKASSIGNED"),
                ("USER_TEMPLATEIDS_SMS", $"{nameof(WebApiConfiguration.User.TemplateIds.Sms.DecisionMade)}", "USER_TEMPLATEIDS_SMS_DECISIONMADE"),

                ("User",           $"{nameof(WebApiConfiguration.User.Whitelist)}",                  "USER_WHITELIST"),
                ("USER_WHITELIST", $"{nameof(WebApiConfiguration.User.Whitelist.ZaakCreate_IDs)}",   "USER_WHITELIST_ZAAKCREATE_IDS"),
                ("USER_WHITELIST", $"{nameof(WebApiConfiguration.User.Whitelist.ZaakUpdate_IDs)}",   "USER_WHITELIST_ZAAKUPDATE_IDS"),
                ("USER_WHITELIST", $"{nameof(WebApiConfiguration.User.Whitelist.ZaakClose_IDs)}",    "USER_WHITELIST_ZAAKCLOSE_IDS"),
                ("USER_WHITELIST", $"{nameof(WebApiConfiguration.User.Whitelist.TaskAssigned_IDs)}", "USER_WHITELIST_TASKASSIGNED_IDS"),
                ("USER_WHITELIST", $"{nameof(WebApiConfiguration.User.Whitelist.DecisionMade_IDs)}", "USER_WHITELIST_DECISIONMADE_IDS"),
                ("USER_WHITELIST", $"{nameof(WebApiConfiguration.User.Whitelist.Message_Allowed)}",  "USER_WHITELIST_MESSAGE_ALLOWED")
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

        internal static WebApiConfiguration GetValidEnvironmentConfiguration()
        {
            return GetWebApiConfiguration(LoaderTypes.Environment, isValid: true);
        }
        #endregion
    }
}