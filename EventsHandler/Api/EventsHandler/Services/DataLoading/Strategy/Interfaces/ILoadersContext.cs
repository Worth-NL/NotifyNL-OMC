// © 2024, Worth Systems.

using EventsHandler.Services.DataLoading.Enums;
using EventsHandler.Services.DataLoading.Interfaces;

namespace EventsHandler.Services.DataLoading.Strategy.Interfaces
{
    /// <summary>
    /// The strategy context for <see cref="ILoadingService"/> strategies - acting like a facade for a specific Data Access Object (DAO) providers.
    /// <para>
    ///   The reason for having multiple DAO is that some non-confidential configurations are stored in
    ///   the public "appsettings.json" file, while others (confidential and vulnerable) might be loaded
    ///   i.e., from Azure Key Vault, Environment Variables, or any other type of data carrier.
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