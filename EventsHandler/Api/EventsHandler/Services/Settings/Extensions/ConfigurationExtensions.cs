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

        private static readonly string s_fallbackByteValue = $"{default(byte)}";
        private static string? s_omcWorkflowVersionKey;
        private static byte? s_omcWorkflowVersionValue;

        /// <summary>
        /// Gets the version of Open services ("OpenNotificaties", "OpenZaak", "OpenKlant") which should be used in business logic.
        /// </summary>
        internal static byte OmcWorkflowVersion()
        {
            s_omcWorkflowVersionKey ??= ($"{nameof(WebApiConfiguration.OMC)}_" +
                                         $"{nameof(WebApiConfiguration.OMC.Features)}_" +
                                         $"{nameof(WebApiConfiguration.OMC.Features.Workflow_Version)}")
                                        .ToUpper();

            return s_omcWorkflowVersionValue ??=
                   byte.Parse(
                       Environment.GetEnvironmentVariable(s_omcWorkflowVersionKey) ?? s_fallbackByteValue);
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
                    ? throw new ArgumentException(string.Format(Resources.Configuration_ERROR_ValueNotFoundOrEmpty, key))
                    : value,

                object objectValue => objectValue == null
                    ? throw new ArgumentException(string.Format(Resources.Configuration_ERROR_ValueNotFoundOrEmpty, key))
                    : value,

                _ => value ??  // Valid value
                     throw new ArgumentException(string.Format(Resources.Configuration_ERROR_ValueNotFoundOrEmpty, key))
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
                : throw new ArgumentException(string.Format(Resources.Configuration_ERROR_ContainsHttp, value));
        }

        /// <summary>
        /// Ensures that the value from the configuration file does not contain API endpoint.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        internal static string GetWithoutEndpoint(this string value)
        {
            return !value.Contains('/')
                ? value
                : throw new ArgumentException(string.Format(Resources.Configuration_ERROR_ContainsEndpoint, value));
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
                : throw new ArgumentException(string.Format(Resources.Configuration_ERROR_InvalidTemplateId, value));
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
                : throw new ArgumentException(string.Format(Resources.Configuration_ERROR_InvalidUri, value));
        }
        #endregion
    }
}