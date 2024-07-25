// © 2024, Worth Systems.

using EventsHandler.Services.Settings.Enums;
using EventsHandler.Services.Settings.Interfaces;

namespace EventsHandler.Services.Settings.Strategy.Interfaces
{
    /// <summary>
    /// The strategy which purpose is to retrieve data using a specific <see cref="ILoadingService"/> data providers.
    /// <para>
    ///   The reason for having multiple data providers is that some non-confidential configurations are stored
    ///   in the public "appsettings.json" file, while others (confidential and vulnerable) might be loaded e.g.,
    ///   from Azure Key Vault, Environment Variables, or any other type of data carrier.
    /// </para>
    /// <para>
    ///   The proposed solution also gives more flexibility regarding how the application can be set up.
    /// </para>
    /// </summary>
    public interface ILoadersContext : ILoadingService
    {
        /// <summary>
        /// Sets the specific DAO (Data Access Object) data provider / aka. "Loader".
        /// </summary>
        /// <param name="loaderType">The specific loader to be set.</param>
        internal void SetLoader(LoaderTypes loaderType);
    }
}