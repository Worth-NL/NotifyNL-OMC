﻿// © 2024, Worth Systems.

namespace Common.Settings.Interfaces
{
    /// <summary>
    /// The service responsible for loading a specific data from an associated resource.
    /// <para>
    ///   It works similarly to Data Access Object (DAO) structure.
    /// </para>
    /// </summary>
    public interface ILoadingService
    {
        /// <summary>
        /// The "AppSettings" node present in "appsettings.json" configuration file, or when mapping "appsettings.json" => environment variables.
        /// </summary>
        protected internal const string AppSettings = nameof(AppSettings);

        /// <summary>
        /// Loads a generic type of data using the dedicated <see langword="string"/> key.
        /// </summary>
        /// <typeparam name="TData">The generic type of data to be returned.</typeparam>
        /// <param name="key">The key to be used to check up a specific value.</param>
        /// <param name="disableValidation">Turns ON/OFF the validation of retrieved configuration value.</param>
        /// <returns>
        ///   The generic data value associated with the key.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        ///   The provided key is missing or invalid.
        /// </exception>
        public TData GetData<TData>(string key, bool disableValidation)
            where TData : notnull;

        /// <summary>
        /// Combines the configuration predeceasing path with the specific node name.
        /// </summary>
        /// <param name="currentPath">The current configuration path.</param>
        /// <param name="nodeName">The name of the configuration node.</param>
        /// <returns>
        ///   The formatted node path.
        /// </returns>
        public string GetPathWithNode(string currentPath, string nodeName);

        /// <summary>
        /// Precedes the (eventually formatted) node name with a respective separator.
        /// </summary>
        /// <param name="nodeName">The name of the configuration node.</param>
        /// <returns>
        ///   The formatted node path.
        /// </returns>
        public string GetNodePath(string nodeName);
    }
}