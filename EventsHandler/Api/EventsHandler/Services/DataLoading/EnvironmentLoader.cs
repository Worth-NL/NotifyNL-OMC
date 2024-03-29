﻿// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Properties;
using EventsHandler.Services.DataLoading.Base;
using EventsHandler.Services.DataLoading.Interfaces;

namespace EventsHandler.Services.DataLoading
{
    /// <inheritdoc cref="ILoadingService"/>
    internal sealed class EnvironmentLoader : BaseLoader
    {
        #region Polymorphism
        /// <inheritdoc cref="ILoadingService.GetData{T}(string)"/>
        /// <exception cref="NotImplementedException">The operating system (OS) is not supported.</exception>
        protected override TData GetData<TData>(string key)
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
                object value = Environment.GetEnvironmentVariable(key)
                    ?? throw new KeyNotFoundException(Resources.Configuration_ERROR_EnvironmentVariableGetNull + key.Separated());

                return (TData)Convert.ChangeType(value, typeof(TData));
            }

            throw new NotImplementedException(Resources.Configuration_ERROR_EnvironmentNotSupported);
        }

        /// <inheritdoc cref="BaseLoader.GetPathWithNode(string, string)"/>
        protected override string GetPathWithNode(string currentPath, string nodeName)
        {
            return $"{currentPath.ToUpper()}{(string.IsNullOrWhiteSpace(nodeName)
                ? string.Empty
                : GetNodePath(nodeName))}";
        }

        /// <inheritdoc cref="BaseLoader.GetNodePath(string)"/>
        protected override string GetNodePath(string nodeName)
        {
            const string separator = "_";

            return $"{separator}{nodeName.ToUpper()}";
        }
        #endregion
    }
}
