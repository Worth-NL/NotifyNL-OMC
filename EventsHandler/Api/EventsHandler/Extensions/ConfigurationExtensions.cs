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
        /// <summary>
        /// Gets the <see cref="LogLevel"/> defined for Application Insights (Azure service).
        /// </summary>
        /// <param name="configuration">The configuration manager to be used.</param>
        /// <returns>
        ///   The value of <see cref="LogLevel"/>.
        /// </returns>
        internal static LogLevel GetApplicationInsightsLogLevel(this IConfiguration configuration)
            => configuration.GetValue<LogLevel>("Logging:ApplicationInsights:LogLevel:Default");

        internal static bool IsEncryptionAsymmetric(this IConfiguration configuration)
            => configuration.GetValue<bool>("Encryption:IsAsymmetric");

        internal static string GetNotifyJwtSecret()
            => Environment.GetEnvironmentVariable("NOTIFY_AUTHORIZATION_JWT_SECRET").NotEmpty("Notify JWT secret");

        internal static string GetNotifyJwtIssuer()
            => Environment.GetEnvironmentVariable("NOTIFY_AUTHORIZATION_JWT_ISSUER").NotEmpty("Notify JWT issuer");

        internal static string GetNotifyJwtAudience()
            => Environment.GetEnvironmentVariable("NOTIFY_AUTHORIZATION_JWT_AUDIENCE").NotEmpty("Notify JWT audience");

        /// <summary>
        /// Gets the <see langword="string"/> value from the configuration.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        internal static T NotEmpty<T>(this T? value, string key)
        {
            if (value is string stringValue)
            {
                return string.IsNullOrWhiteSpace(stringValue)
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

        internal static string Separated(this string text) => $" {text}";
    }
}