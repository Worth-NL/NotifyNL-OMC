// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Properties;
using EventsHandler.Services.Settings.Interfaces;

namespace EventsHandler.Services.Settings
{
    /// <inheritdoc cref="ILoadingService"/>
    /// <remarks>
    ///   This data provider is using "appsettings.json" configuration file.
    /// </remarks>
    internal class AppSettingsLoader : ILoadingService
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettingsLoader"/> class.
        /// </summary>
        public AppSettingsLoader(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        #region Polymorphism
        /// <inheritdoc cref="ILoadingService.GetData{T}(string, bool)"/>
        /// <exception cref="ArgumentException"/>
        TData ILoadingService.GetData<TData>(string key, bool disableValidation)
        {
            // The key is missing
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new KeyNotFoundException(Resources.Configuration_ERROR_InvalidKey);
            }

            TData? value = this._configuration.GetValue<TData>(key);
            value = disableValidation
                ? value ?? default!
                : value.ValidateNotEmpty(key);

            return value;
        }

        /// <inheritdoc cref="ILoadingService.GetPathWithNode(string, string)"/>
        string ILoadingService.GetPathWithNode(string currentPath, string nodeName)
        {
            if (string.IsNullOrWhiteSpace(currentPath))
            {
                return string.Empty;
            }

            if (currentPath == "AppSettings")  // Skip "AppSettings" as part of the configuration path
            {
                return nodeName;
            }

            return $"{currentPath}" +
                   $"{((ILoadingService)this).GetNodePath(nodeName)}";
        }

        /// <inheritdoc cref="ILoadingService.GetNodePath(string)"/>
        string ILoadingService.GetNodePath(string nodeName)
        {
            if (string.IsNullOrWhiteSpace(nodeName))
            {
                return string.Empty;
            }

            const string separator = ":";

            return $"{separator}{nodeName}";
        }
        #endregion
    }
}