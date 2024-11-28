// © 2024, Worth Systems.

using Common.Extensions;
using Common.Properties;
using Common.Settings.DAO;
using Common.Settings.DAO.Interfaces;
using Common.Settings.Extensions;
using Common.Settings.Interfaces;

namespace Common.Settings
{
    /// <inheritdoc cref="ILoadingService"/>
    /// <remarks>
    ///   This data provider is using system's Environment Variables.
    /// </remarks>
    public class EnvironmentLoader : ILoadingService
    {
        /// <inheritdoc cref="IEnvironment"/>
        public IEnvironment Environment { get; init; } = new EnvironmentReader();

        #region Polymorphism
        /// <inheritdoc cref="ILoadingService.GetData{T}(string, bool)"/>
        /// <exception cref="NotImplementedException">The operating system (OS) is not supported.</exception>
        TData ILoadingService.GetData<TData>(string key, bool disableValidation)
        {
            // The key is missing
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new KeyNotFoundException(AppResources.Configuration_ERROR_InvalidKey);
            }

            string? value = this.Environment.GetEnvironmentVariable(key);
            value = disableValidation
                ? value ?? string.Empty
                : value.GetNotEmpty(key);

            return value.ChangeType<TData>();
        }

        /// <inheritdoc cref="ILoadingService.GetPathWithNode(string, string)"/>
        string ILoadingService.GetPathWithNode(string currentPath, string nodeName)
        {
            if (string.IsNullOrWhiteSpace(currentPath))
            {
                return string.Empty;
            }

            // Skip "AppSettings" as part of the configuration path (during mapping to environment variables)
            if (currentPath == ILoadingService.AppSettings)
            {
                return nodeName;
            }

            return $"{currentPath.ToUpper()}" +
                   $"{((ILoadingService)this).GetNodePath(nodeName)}";
        }

        /// <inheritdoc cref="ILoadingService.GetNodePath(string)"/>
        string ILoadingService.GetNodePath(string nodeName)
        {
            if (string.IsNullOrWhiteSpace(nodeName))
            {
                return string.Empty;
            }

            const string separator = "_";

            return $"{separator}{nodeName.ToUpper()}";
        }
        #endregion
    }
}