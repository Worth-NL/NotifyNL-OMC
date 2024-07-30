// © 2023, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Properties;
using EventsHandler.Services.Settings.Configuration;
using System.Collections;
using System.Text.RegularExpressions;

namespace EventsHandler.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IConfiguration"/> used in <see cref="Program"/>.cs
    /// where the <see cref="WebApiConfiguration"/> service is not existing yet so, it could
    /// not be retrieved from <see cref="IServiceCollection"/>.
    /// </summary>
    internal static partial class ConfigurationExtensions  // NOTE: "partial" is introduced by the new RegEx generation approach
    {
        #region GetValue<T>
        /// <summary>
        /// Gets type of encryption used in the application for JWT tokens.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        internal static bool IsEncryptionAsymmetric(this IConfiguration configuration)
            => configuration.GetValue<bool>(key: $"{nameof(WebApiConfiguration.AppSettings.Encryption)}:" +
                                                 $"{nameof(WebApiConfiguration.AppSettings.Encryption.IsAsymmetric)}");

        /// <summary>
        /// Gets the version of Open services ("OpenNotificaties", "OpenZaak", "OpenKlant") which should be used in business logic.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        internal static byte OmcWorkflowVersion(this IConfiguration configuration)
            => configuration.GetValue<byte>(key: $"{nameof(WebApiConfiguration.AppSettings.Features)}:" +
                                                 $"{nameof(WebApiConfiguration.AppSettings.Features.OmcWorkflowVersion)}");
        #endregion

        #region Validation
        /// <summary>
        /// Gets the <see langword="string"/> value from the configuration.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        internal static T ValidateNotEmpty<T>(this T? value, string key)
        {
            return value switch
            {
                string stringValue => string.IsNullOrEmpty(stringValue)
                    ? throw new ArgumentException(string.Format(Resources.Configuration_ERROR_ValueNotFoundOrEmpty, key))
                    : value,

                ICollection collection => collection.IsEmpty()
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
        internal static string ValidateNoHttp(this string value)
        {
            return !value.StartsWith(DefaultValues.Request.HttpProtocol)
                ? value
                : throw new ArgumentException(string.Format(Resources.Configuration_ERROR_ContainsHttp, value));
        }

        /// <summary>
        /// Ensures that the value from the configuration file does not contain API endpoint.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        internal static string ValidateNoEndpoint(this string value)
        {
            return !value.Contains('/')
                ? value
                : throw new ArgumentException(string.Format(Resources.Configuration_ERROR_ContainsEndpoint, value));
        }

        [GeneratedRegex(@"^\w{8}\-\w{4}\-\w{4}\-\w{4}\-\w{12}$", RegexOptions.Compiled)]
        private static partial Regex TemplateIdRegex();

        /// <summary>
        /// Ensures that the value from the configuration file conforms format of Template Id.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        internal static string ValidateTemplateId(this string value)
        {
            return TemplateIdRegex().Match(value).Success
                ? value
                : throw new ArgumentException(string.Format(Resources.Configuration_ERROR_InvalidTemplateId, value));
        }
        
        /// <summary>
        /// Ensures that the value from the configuration file is a valid <see cref="Uri"/> address.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        internal static Uri ValidateUri(this Uri value)
        {
            return value != DefaultValues.Models.EmptyUri
                ? value
                : throw new ArgumentException(string.Format(Resources.Configuration_ERROR_InvalidUri, value));
        }
        #endregion
    }
}