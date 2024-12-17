// © 2023, Worth Systems.

using Common.Constants;
using Common.Properties;
using Common.Settings.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Settings.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IConfiguration"/> used in other projects.
    /// where the <see cref="OmcConfiguration"/> service is not existing yet so, it could
    /// not be retrieved from <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ConfigExtensions
    {
        #region Environment variable names
        private static readonly object s_padlock = new();

        /// <summary>
        /// The "ASPNETCORE_ENVIRONMENT" environment variable.
        /// </summary>
        public static string AspNetCoreEnvironment => "ASPNETCORE_ENVIRONMENT";
            
        /// <summary>
        /// The "SENTRY_DSN" environment variable.
        /// </summary>
        public static string SentryDsn => "SENTRY_DSN";

        // TODO: Most of these environment variable names can be objectified in WebApiConfiguration
        /// <summary>
        /// The "SENTRY_ENVIRONMENT" environment variable.
        /// </summary>
        public static string SentryEnvironment => "SENTRY_ENVIRONMENT";

        private static string? s_openZaakDomainEnvVarName;

        private static string GetEndpointOpenZaakEnvVarName()
        {
            lock (s_padlock)
            {
                return s_openZaakDomainEnvVarName ??= ($"{nameof(OmcConfiguration.ZGW)}_" +
                                                       $"{nameof(OmcConfiguration.ZGW.Endpoint)}_" +
                                                       $"{nameof(OmcConfiguration.ZGW.Endpoint.OpenZaak)}")
                                                      .ToUpper();
            }
        }

        private static string? s_openKlantDomainEnvVarName;

        private static string GetEndpointOpenKlantEnvVarName()
        {
            lock (s_padlock)
            {
                return s_openKlantDomainEnvVarName ??= ($"{nameof(OmcConfiguration.ZGW)}_" +
                                                        $"{nameof(OmcConfiguration.ZGW.Endpoint)}_" +
                                                        $"{nameof(OmcConfiguration.ZGW.Endpoint.OpenKlant)}")
                                                       .ToUpper();
            }
        }

        private static string? s_messageAllowedEnvVarName;
        
        /// <summary>
        /// The "ZGW_WHITELIST_MESSAGEALLOWED" environment variable.
        /// </summary>
        public static string GetWhitelistMessageAllowedEnvVarName()
        {
            lock (s_padlock)
            {
                return s_messageAllowedEnvVarName ??= ($"{nameof(OmcConfiguration.ZGW)}_" +
                                                       $"{nameof(OmcConfiguration.ZGW.Whitelist)}_" +
                                                       $"{nameof(OmcConfiguration.ZGW.Whitelist.Message_Allowed)}")
                                                      .ToUpper();
            }
        }

        private static string? s_genObjectTypeEnvVarName;

        /// <summary>
        /// The "ZGW_VARIABLE_OBJECTTYPE_..._OBJECTTYPE_UUID" environment variable.
        /// </summary>
        public static string GetGenericVariableObjectTypeEnvVarName()
        {
            lock (s_padlock)
            {
                return s_genObjectTypeEnvVarName ??= ($"{nameof(OmcConfiguration.ZGW)}_" +
                                                      $"{nameof(OmcConfiguration.ZGW.Variable)}_" +
                                                      $"{nameof(OmcConfiguration.ZGW.Variable.ObjectType)}_" +
                                                      $"...OBJECTTYPE_UUID")
                                                     .ToUpper();
            }
        }

        private static string? s_infoObjectTypesEnvVarName;
        
        /// <summary>
        /// The "ZGW_VARIABLE_OBJECTTYPE_DECISIONINFOOBJECTTYPE_UUIDS" environment variable.
        /// </summary>
        public static string GetVariableInfoObjectsEnvVarName()
        {
            lock (s_padlock)
            {
                return s_infoObjectTypesEnvVarName ??= ($"{nameof(OmcConfiguration.ZGW)}_" +
                                                        $"{nameof(OmcConfiguration.ZGW.Variable)}_" +
                                                        $"{nameof(OmcConfiguration.ZGW.Variable.ObjectType)}_" +
                                                        $"{nameof(OmcConfiguration.ZGW.Variable.ObjectType.DecisionInfoObjectType_Uuids)}")
                                                       .ToUpper();
            }
        }
        #endregion

        #region GetValue<T>
        /// <summary>
        /// Gets type of encryption used in the application for JWT tokens.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        public static bool IsEncryptionAsymmetric(this IConfiguration configuration)
        {
            const string key = $"{nameof(OmcConfiguration.AppSettings.Encryption)}:" +
                               $"{nameof(OmcConfiguration.AppSettings.Encryption.IsAsymmetric)}";

            return configuration.GetValue<bool>(key);
        }

        private static string? s_openZaakDomainValue;

        /// <summary>
        /// Gets the OpenZaak domain.
        /// </summary>
        public static string OpenZaakDomain(OmcConfiguration? configuration = null)
        {
            // Case #1: Instance usage
            if (configuration != null)
            {
                return configuration.ZGW.Endpoint.OpenZaak();
            }

            // Case #2: Static usage
            return s_openZaakDomainValue ??=
                Environment.GetEnvironmentVariable(GetEndpointOpenZaakEnvVarName()) ?? "x";
        }

        private static string? s_openKlantDomainValue;
        
        /// <summary>
        /// Gets the OpenKlant domain.
        /// </summary>
        public static string OpenKlantDomain(OmcConfiguration? configuration = null)
        {
            // Case #1: Instance usage
            if (configuration != null)
            {
                return configuration.ZGW.Endpoint.OpenKlant();
            }

            // Case #2: Static usage
            return s_openKlantDomainValue ??=
                Environment.GetEnvironmentVariable(GetEndpointOpenKlantEnvVarName()) ?? "x";
        }
        #endregion

        #region Validation
        /// <summary>
        /// Gets the <see langword="string"/> value from the configuration.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static T GetNotEmpty<T>(this T? value, string key)
        {
            return value switch
            {
                string stringValue => string.IsNullOrWhiteSpace(stringValue)
                    ? ThrowArgumentException<T>(CommonResources.Configuration_ERROR_ValueNotFoundOrEmpty, key)
                    : value,

                _ => value ??  // Valid value
                     ThrowArgumentException<T>(CommonResources.Configuration_ERROR_ValueNotFoundOrEmpty, key)
            };
        }

        /// <summary>
        /// Ensures that the value from the configuration file does not contain "http" or "https" protocol.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static string GetWithoutProtocol(this string value)
        {
            return !value.StartsWith(CommonValues.Default.Network.HttpProtocol)  // HTTPS will be also handled this way
                ? value
                : ThrowArgumentException<string>(CommonResources.Configuration_ERROR_ContainsHttp, value);
        }
        #endregion

        #region Conversion attempt
        /// <summary>
        /// Ensures that the value from the configuration file conforms format of Template Id.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static Guid GetValidGuid(this string value)
        {
            return Guid.TryParse(value, out Guid createdGuid)
                ? createdGuid
                : ThrowArgumentException<Guid>(CommonResources.Configuration_ERROR_InvalidTemplateId, value);
        }
        
        /// <summary>
        /// Ensures that the value from the configuration file is a valid <see cref="Uri"/> address.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static Uri GetValidUri(this string value)
        {
            return Uri.TryCreate(value, UriKind.Absolute, out Uri? createdUri) &&
                   createdUri != CommonValues.Default.Models.EmptyUri
                ? createdUri
                : ThrowArgumentException<Uri>(CommonResources.Configuration_ERROR_InvalidUri, value);
        }
        #endregion

        #region Helper methods
        private static T ThrowArgumentException<T>(string errorMessage, string key)
        {
            throw new ArgumentException(string.Format(errorMessage, key));
        }
        #endregion
    }
}