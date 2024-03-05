// © 2024, Worth Systems.

using EventsHandler.Properties;
using EventsHandler.Services.DataLoading.Enums;
using EventsHandler.Services.DataLoading.Interfaces;
using EventsHandler.Services.DataLoading.Strategy.Interfaces;

namespace EventsHandler.Services.DataLoading.Strategy.Manager
{
    /// <inheritdoc cref="ILoadersContext"/>
    internal sealed class LoadersContext : ILoadersContext
    {
        private readonly IServiceProvider _serviceProvider;

        private ILoadingService _loadingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadersContext"/> class.
        /// </summary>
        public LoadersContext(IServiceProvider serviceProvider, ILoadingService loadingService)  // NOTE: Used by Dependency Injection
        {
            this._serviceProvider = serviceProvider;
            this._loadingService = loadingService;
        }

        #region ILoadersContext
        /// <inheritdoc cref="ILoadersContext.SetLoader(LoaderTypes)"/>
        void ILoadersContext.SetLoader(LoaderTypes loaderType)
        {
            this._loadingService = loaderType switch
            {
                // Reading configurations from "appsettings.json" (Dev, Test, Prod, and the fallback general file)
                LoaderTypes.Configuration => this._serviceProvider.GetRequiredService<ConfigurationLoader>(),

                // Reading configurations from the preset environment variables (e.g. in Windows, Linus, macOS)
                LoaderTypes.Environment => this._serviceProvider.GetRequiredService<EnvironmentLoader>(),

                _ => throw new NotImplementedException(Resources.Processing_ERROR_Loader_NotImplemented)
            };
        }
        #endregion

        #region ILoadingService
        /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
        TData ILoadingService.GetData<TData>(string key)
        {
            return this._loadingService.GetData<TData>(key);
        }
        
        /// <inheritdoc cref="ILoadingService.GetPathWithNode(string, string)"/>
        string ILoadingService.GetPathWithNode(string currentPath, string nodeName)
        {
            return this._loadingService.GetPathWithNode(currentPath, nodeName);
        }
        #endregion
    }
}
