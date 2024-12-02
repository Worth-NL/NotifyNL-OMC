// © 2023, Worth Systems.

using Common.Settings.Configuration;
using EventsHandler.Constants;
using EventsHandler.Services.DataSending.Clients.Factories.Interfaces;
using System.Net.Http.Headers;

namespace EventsHandler.Services.DataSending.Clients.Factories
{
    /// <inheritdoc cref="IHttpClientFactory{THttpClient,TParameter}"/>
    /// <remarks>
    ///   Customized to create a generic .NET <see cref="HttpClient"/>s.
    /// </remarks>
    internal sealed class RegularHttpClientFactory : IHttpClientFactory<HttpClient, (string Name, string Value)[]>
    {
        private readonly WebApiConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegularHttpClientFactory"/> class.
        /// </summary>
        public RegularHttpClientFactory(WebApiConfiguration configuration)
        {
            this._configuration = configuration;
        }

        /// <inheritdoc cref="IHttpClientFactory{THttpClient,TParameters}.GetHttpClient(TParameters)"/>
        HttpClient IHttpClientFactory<HttpClient, (string Name, string Value)[]>.GetHttpClient((string Name, string Value)[] requestHeaders)
        {
            var socketsHandler = new SocketsHttpHandler
            {
                // Prevents DNS changes issue (changing IP address) and leading to stale connections, resource leaks, or timeouts
                PooledConnectionLifetime = TimeSpan.FromSeconds(this._configuration.AppSettings.Network.ConnectionLifetimeInSeconds())
            };

            var httpClient = new HttpClient(socketsHandler)
            {
                // Prevents issues with resource leaks and application performance in case of HTTP Requests taking too long
                Timeout = TimeSpan.FromSeconds(this._configuration.AppSettings.Network.HttpRequestTimeoutInSeconds())
            };

            // Set universal Request Headers
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(ApiValues.Default.ApiController.ContentType));  // Content-Type: application/json

            // Set custom Request Headers
            for (int index = 0; index < requestHeaders.Length; index++)
            {
                httpClient.DefaultRequestHeaders.Add(
                    /* Headers Key   */ requestHeaders[index].Name,
                    /* Headers Value */ requestHeaders[index].Value);
            }

            return httpClient;
        }
    }
}