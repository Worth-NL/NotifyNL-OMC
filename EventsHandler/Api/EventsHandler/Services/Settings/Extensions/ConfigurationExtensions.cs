// © 2023, Worth Systems.

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

        private static string? s_openZaakDomainKey;
        private static string? s_openZaakDomainValue;

        internal static string OpenZaakDomain(WebApiConfiguration? configuration = null)
        {
            // Case #1: Instance usage
            if (configuration != null)
            {
                return configuration.User.Domain.OpenZaak();
            }

            // Case #2: Static usage
            s_openZaakDomainKey ??= ($"{nameof(WebApiConfiguration.User)}_" +
                                     $"{nameof(WebApiConfiguration.User.Domain)}_" +
                                     $"{nameof(WebApiConfiguration.User.Domain.OpenZaak)}")
                                    .ToUpper();

            return s_openZaakDomainValue ??=
                Environment.GetEnvironmentVariable(s_openZaakDomainKey)
                ?? ThrowArgumentException<string>(Resources.Configuration_ERROR_ValueNotFoundOrEmpty, s_openZaakDomainKey);
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

        /// <summary>
        /// Ensures that the value from the configuration file does not contain API endpoint.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        internal static string GetWithoutEndpoint(this string value)
        {
            return !value.Contains('/')
                ? value
                : ThrowArgumentException<string>(Resources.Configuration_ERROR_ContainsEndpoint, value);
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