// © 2023, Worth Systems.

namespace EventsHandler.Services.DataSending.Clients.Factories.Interfaces
{
    /// <summary>
    /// The service to return a specific instance of HTTP Client.
    /// </summary>
    public interface IHttpClientFactory<out THttpClient, in TParameters>
        where THttpClient : class
        where TParameters : notnull
    {
        /// <summary>
        /// Gets the <typeparamref name="THttpClient"/> with customized parameters (e.g. Headers).
        /// </summary>
        /// <param name="parameters">
        ///   The parameters to configure a requested <typeparamref name="THttpClient"/>.
        /// </param>
        /// <returns>
        ///   The customized <typeparamref name="THttpClient"/>.
        /// </returns>
        internal THttpClient GetHttpClient(TParameters parameters);
    }
}