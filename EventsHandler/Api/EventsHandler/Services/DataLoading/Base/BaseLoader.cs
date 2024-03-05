// © 2024, Worth Systems.

using EventsHandler.Services.DataLoading.Interfaces;

namespace EventsHandler.Services.DataLoading.Base
{
    /// <summary>
    /// The base parent class for data provider / <see cref="ILoadingService"/> strategies.
    /// </summary>
    internal abstract class BaseLoader : ILoadingService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseLoader"/> class.
        /// </summary>
        protected BaseLoader()
        {
        }

        #region Interface
        /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
        TData ILoadingService.GetData<TData>(string key)
            => GetData<TData>(key);

        /// <inheritdoc cref="ILoadingService.GetPathWithNode(string, string)"/>
        string ILoadingService.GetPathWithNode(string currentPath, string nodeName)
            => GetPathWithNode(currentPath, nodeName);
        #endregion

        #region Abstract
        /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
        protected abstract TData GetData<TData>(string key);

        /// <inheritdoc cref="ILoadingService.GetPathWithNode(string, string)"/>
        protected abstract string GetPathWithNode(string currentPath, string nodeName);

        /// <summary>
        /// Precedes the (eventually formatted) node name with a respective separator.
        /// </summary>
        /// <param name="nodeName">The name of the configuration node.</param>
        /// <returns>
        ///   The formatted node path.
        /// </returns>
        protected abstract string GetNodePath(string nodeName);
        #endregion
    }
}
