﻿// © 2023, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Properties;
using EventsHandler.Services.Settings.Configuration;

namespace EventsHandler.Services.Settings.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IConfiguration"/> used in <see cref="Program"/>.cs
    /// where the <see cref="WebApiConfiguration"/> service is not existing yet so, it could
    /// not be retrieved from <see cref="IServiceCollection"/>.
    /// </summary>
    internal static class ConfigurationExtensions
    {
        #region Environment variable names
        private static readonly object s_padlock = new();

        private static string? s_openZaakDomainEnvVarName;

        private static string GetEndpointOpenZaakEnvVarName()
        {
            lock (s_padlock)
            {
                return s_openZaakDomainEnvVarName ??= ($"{nameof(WebApiConfiguration.ZGW)}_" +
                                                       $"{nameof(WebApiConfiguration.ZGW.Endpoint)}_" +
                                                       $"{nameof(WebApiConfiguration.ZGW.Endpoint.OpenZaak)}")
                                                      .ToUpper();
            }
        }

        private static string? s_openKlantDomainEnvVarName;

        private static string GetEndpointOpenKlantEnvVarName()
        {
            lock (s_padlock)
            {
                return s_openKlantDomainEnvVarName ??= ($"{nameof(WebApiConfiguration.ZGW)}_" +
                                                        $"{nameof(WebApiConfiguration.ZGW.Endpoint)}_" +
                                                        $"{nameof(WebApiConfiguration.ZGW.Endpoint.OpenKlant)}")
                                                       .ToUpper();
            }
        }

        private static string? s_messageAllowedEnvVarName;

        internal static string GetWhitelistMessageAllowedEnvVarName()
        {
            lock (s_padlock)
            {
                return s_messageAllowedEnvVarName ??= ($"{nameof(WebApiConfiguration.ZGW)}_" +
                                                       $"{nameof(WebApiConfiguration.ZGW.Whitelist)}_" +
                                                       $"{nameof(WebApiConfiguration.ZGW.Whitelist.Message_Allowed)}")
                                                      .ToUpper();
            }
        }

        private static string? s_genObjectTypeEnvVarName;

        internal static string GetGenericVariableObjectTypeEnvVarName()
        {
            lock (s_padlock)
            {
                return s_genObjectTypeEnvVarName ??= ($"{nameof(WebApiConfiguration.ZGW)}_" +
                                                      $"{nameof(WebApiConfiguration.ZGW.Variable)}_" +
                                                      $"{nameof(WebApiConfiguration.ZGW.Variable.ObjectType)}_" +
                                                      $"...OBJECTTYPE_UUID")
                                                     .ToUpper();
            }
        }

        private static string? s_infoObjectTypesEnvVarName;

        internal static string GetVariableInfoObjectsEnvVarName()
        {
            lock (s_padlock)
            {
                return s_infoObjectTypesEnvVarName ??= ($"{nameof(WebApiConfiguration.ZGW)}_" +
                                                        $"{nameof(WebApiConfiguration.ZGW.Variable)}_" +
                                                        $"{nameof(WebApiConfiguration.ZGW.Variable.ObjectType)}_" +
                                                        $"{nameof(WebApiConfiguration.ZGW.Variable.ObjectType.DecisionInfoObjectType_Uuids)}")
                                                       .ToUpper();
            }
        }
        #endregion

        #region GetValue<T>
        /// <summary>
        /// Gets type of encryption used in the application for JWT tokens.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        internal static bool IsEncryptionAsymmetric(this IConfiguration configuration)
        {
            const string key = $"{nameof(WebApiConfiguration.AppSettings.Encryption)}:" +
                               $"{nameof(WebApiConfiguration.AppSettings.Encryption.IsAsymmetric)}";

            return configuration.GetValue<bool>(key);
        }

        private static string? s_openZaakDomainValue;

        internal static string OpenZaakDomain(WebApiConfiguration? configuration = null)
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

        internal static string OpenKlantDomain(WebApiConfiguration? configuration = null)
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
        internal static T GetNotEmpty<T>(this T? value, string key)
        {
            return value switch
            {
                string stringValue => string.IsNullOrWhiteSpace(stringValue)
                    ? ThrowArgumentException<T>(Resources.Configuration_ERROR_ValueNotFoundOrEmpty, key)
                    : value,

                _ => value ??  // Valid value
                     ThrowArgumentException<T>(Resources.Configuration_ERROR_ValueNotFoundOrEmpty, key)
            };
        }

        /// <summary>
        /// Ensures that the value from the configuration file does not contain "http" or "https" protocol.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        internal static string GetWithoutProtocol(this string value)
        {
            return !value.StartsWith(DefaultValues.Request.HttpProtocol)  // HTTPS will be also handled this way
                ? value
                : ThrowArgumentException<string>(Resources.Configuration_ERROR_ContainsHttp, value);
        }
        #endregion

        #region Conversion attempt
        /// <summary>
        /// Ensures that the value from the configuration file conforms format of Template Id.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        internal static Guid GetValidGuid(this string value)
        {
            return Guid.TryParse(value, out Guid createdGuid)
                ? createdGuid
                : ThrowArgumentException<Guid>(Resources.Configuration_ERROR_InvalidTemplateId, value);
        }
        
        /// <summary>
        /// Ensures that the value from the configuration file is a valid <see cref="Uri"/> address.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        internal static Uri GetValidUri(this string value)
        {
            return Uri.TryCreate(value, UriKind.Absolute, out Uri? createdUri) &&
                   createdUri != DefaultValues.Models.EmptyUri
                ? createdUri
                : ThrowArgumentException<Uri>(Resources.Configuration_ERROR_InvalidUri, value);
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