// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Properties;
using EventsHandler.Services.Settings.Interfaces;

namespace EventsHandler.Services.Settings
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    /// <inheritdoc cref="ILoadingService"/>
    /// <remarks>
    ///   This data provider is using system's Environment Variables.
    /// </remarks>
    internal class EnvironmentLoader : ILoadingService
    {
        #region Polymorphism
        /// <inheritdoc cref="ILoadingService.GetData{T}(string)"/>
        /// <exception cref="NotImplementedException">The operating system (OS) is not supported.</exception>
        public virtual TData GetData<TData>(string key)
        {
            // The key is missing
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new KeyNotFoundException(Resources.Configuration_ERROR_EnvironmentVariableGetNull + key.Separated());
            }

            // Support for modern Windows OS (e.g., Windows XP, Vista, 7, 8+, 10, 11), macOS, and Unix systems (e.g. Linux)
            if (Environment.OSVersion.Platform is PlatformID.Win32NT
                                               or PlatformID.MacOSX
                                               or PlatformID.Unix)
            {
                // The value is null
                object value = Environment.GetEnvironmentVariable(key)
                    ?? throw new KeyNotFoundException(Resources.Configuration_ERROR_EnvironmentVariableGetNull + key.Separated());

                return (TData)Convert.ChangeType(value, typeof(TData));
            }

            throw new NotImplementedException(Resources.Configuration_ERROR_EnvironmentNotSupported);
        }

        /// <inheritdoc cref="ILoadingService.GetPathWithNode(string, string)"/>
        public virtual string GetPathWithNode(string currentPath, string nodeName)
        {
            return $"{currentPath.ToUpper()}{(string.IsNullOrWhiteSpace(nodeName)
                ? string.Empty
                : GetNodePath(nodeName))}";
        }

        /// <inheritdoc cref="ILoadingService.GetNodePath(string)"/>
        public virtual string GetNodePath(string nodeName)
        {
            const string separator = "_";

            return $"{separator}{nodeName.ToUpper()}";
        }
        #endregion
    }
}