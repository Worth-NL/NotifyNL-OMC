// © 2024, Worth Systems.

using Common.Properties;
using Common.Settings.Extensions;
using Common.Settings.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Common.Settings
{
    /// <inheritdoc cref="ILoadingService"/>
    /// <remarks>
    ///   This data provider is using "appsettings.json" configuration file.
    /// </remarks>
    public class AppSettingsLoader : ILoadingService
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettingsLoader"/> class.
        /// </summary>
        public AppSettingsLoader(IConfiguration configuration)  // Dependency Injection (DI)
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
                throw new KeyNotFoundException(AppResources.Configuration_ERROR_InvalidKey);
            }

            TData? value = this._configuration.GetValue<TData>(key);
            value = disableValidation
                ? value ?? default!
                : value.GetNotEmpty(key);

            return value;
        }

        /// <inheritdoc cref="ILoadingService.GetPathWithNode(string, string)"/>
        string ILoadingService.GetPathWithNode(string currentPath, string nodeName)
        {
            if (string.IsNullOrWhiteSpace(currentPath))
            {
                return string.Empty;
            }

            // Skip "AppSettings" as part of the configuration path
            if (currentPath == ILoadingService.AppSettings)
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