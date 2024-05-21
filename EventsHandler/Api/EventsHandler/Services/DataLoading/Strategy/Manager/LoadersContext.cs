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
                LoaderTypes.AppSettings => this._serviceProvider.GetRequiredService<AppSettingsLoader>(),

                // Reading configurations from the preset environment variables (e.g. in Windows, Linux, macOS)
                LoaderTypes.Environment => this._serviceProvider.GetRequiredService<EnvironmentLoader>(),

                _ => throw new NotImplementedException(Resources.Processing_ERROR_Loader_NotImplemented)
            };
        }

        /// <inheritdoc cref="ILoadersContext.GetLoaderType()"/>
        LoaderTypes ILoadersContext.GetLoaderType()
        {
            return this._loadingService switch
            {
                AppSettingsLoader => LoaderTypes.AppSettings,

                EnvironmentLoader => LoaderTypes.Environment,

                _ => throw new NotImplementedException(Resources.Processing_ERROR_Loader_NotImplemented)
            };
        }
        #endregion

        #region ILoadingService
        /// <inheritdoc cref="ILoadingService.GetData{TData}(string)"/>
        TData ILoadingService.GetData<TData>(string key)
            => this._loadingService.GetData<TData>(key);

        /// <inheritdoc cref="ILoadingService.GetPathWithNode(string, string)"/>
        string ILoadingService.GetPathWithNode(string currentPath, string nodeName)
            => this._loadingService.GetPathWithNode(currentPath, nodeName);

        /// <inheritdoc cref="ILoadingService.GetNodePath(string)"/>
        string ILoadingService.GetNodePath(string nodeName)
            => this._loadingService.GetNodePath(nodeName);
        #endregion
    }
}