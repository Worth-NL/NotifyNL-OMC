// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Properties;
using EventsHandler.Services.DataLoading.Base;
using EventsHandler.Services.DataLoading.Interfaces;

namespace EventsHandler.Services.DataLoading
{
    /// <inheritdoc cref="ILoadingService"/>
    internal sealed class ConfigurationLoader : BaseLoader
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
        protected override TData GetData<TData>(string key)
        {
            // The key is missing
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new KeyNotFoundException(Resources.Configuration_ERROR_ValueNotFoundOrEmpty);
            }

            return this._configuration.GetValue<TData>(key).NotEmpty(key);
        }

        /// <inheritdoc cref="BaseLoader.GetPathWithNode(string, string)"/>
        protected override string GetPathWithNode(string currentPath, string nodeName)
        {
            return $"{currentPath}{(string.IsNullOrWhiteSpace(nodeName)
                ? string.Empty
                : GetNodePath(nodeName))}";
        }

        /// <inheritdoc cref="BaseLoader.GetNodePath(string)"/>
        protected override string GetNodePath(string nodeName)
        {
            const string separator = ":";

            return $"{separator}{nodeName}";
        }
        #endregion
    }
}
