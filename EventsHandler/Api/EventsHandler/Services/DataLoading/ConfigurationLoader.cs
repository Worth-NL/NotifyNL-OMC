// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Properties;
using EventsHandler.Services.DataLoading.Interfaces;

namespace EventsHandler.Services.DataLoading
{
    /// <inheritdoc cref="ILoadingService"/>
    internal sealed class ConfigurationLoader : ILoadingService
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationLoader"/> class.
        /// </summary>
        public ConfigurationLoader(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        #region Polymorphism
        /// <inheritdoc cref="ILoadingService.GetData{T}(string)"/>
        /// <exception cref="ArgumentException"/>
        TData ILoadingService.GetData<TData>(string key)
        {
            // The key is missing
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new KeyNotFoundException(Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            }

            return this._configuration.GetValue<TData>(key).NotEmpty(key);
        }

        /// <inheritdoc cref="ILoadingService.GetPathWithNode(string, string)"/>
        string ILoadingService.GetPathWithNode(string currentPath, string nodeName)
        {
            return $"{currentPath}{(string.IsNullOrWhiteSpace(nodeName)
                ? string.Empty
                : ((ILoadingService)this).GetNodePath(nodeName))}";
        }

        /// <inheritdoc cref="ILoadingService.GetNodePath(string)"/>
        string ILoadingService.GetNodePath(string nodeName)
        {
            const string separator = ":";

            return $"{separator}{nodeName}";
        }
        #endregion
    }
}
