// © 2023, Worth Systems.

using EventsHandler.Configuration;
using EventsHandler.Constants;
using EventsHandler.Properties;
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
        internal static int OpenServicesVersion(this IConfiguration configuration)
            => configuration.GetValue<int>(key: $"{nameof(WebApiConfiguration.AppSettings.Features)}:" +
                                                $"{nameof(WebApiConfiguration.AppSettings.Features.OpenServicesVersion)}");
        #endregion

        #region Validation
        /// <summary>
        /// Gets the <see langword="string"/> value from the configuration.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        internal static T NotEmpty<T>(this T? value, string key)
        {
            if (value is string stringValue)
            {
                return string.IsNullOrEmpty(stringValue)
                    ? throw new ArgumentException(Resources.Configuration_ERROR_ValueNotFoundOrEmpty + Separated(key))
                    : value;
            }

            return value ?? throw new ArgumentException(Resources.Configuration_ERROR_ValueNotFoundOrEmpty + Separated(key));
        }

        /// <summary>
        /// Ensures that the value from configuration file does not contain "http" or "https" protocol.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        internal static string WithoutHttp(this string value)
        {
            return !value.StartsWith(DefaultValues.Request.HttpProtocol)
                ? value
                : throw new ArgumentException(Resources.Configuration_ERROR_ContainsHttp + Separated(value));
        }

        /// <summary>
        /// Ensures that the value from configuration file does not contain API endpoint.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        internal static string WithoutEndpoint(this string value)
        {
            return !value.Contains('/')
                ? value
                : throw new ArgumentException(Resources.Configuration_ERROR_ContainsEndpoint + Separated(value));
        }

        [GeneratedRegex(@"^\w{8}\-\w{4}\-\w{4}\-\w{4}\-\w{12}$", RegexOptions.Compiled)]
        private static partial Regex TemplateIdRegex();

        /// <summary>
        /// Ensures that the value from configuration file conforms format of Template Id.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        internal static string ValidTemplateId(this string value)
        {
            return TemplateIdRegex().Match(value).Success
                ? value
                : throw new ArgumentException(Resources.Configuration_ERROR_InvalidTemplateId + Separated(value));
        }
        #endregion

        #region Formatting
        internal static string Separated(this string text) => $" {text}";
        #endregion
    }
}