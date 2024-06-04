// © 2023, Worth Systems.

using EventsHandler.Configuration;
using EventsHandler.Constants;
using EventsHandler.Services.DataReceiving.Factories.Interfaces;
using System.Net.Http.Headers;

namespace EventsHandler.Services.DataReceiving.Factories
{
    /// <inheritdoc cref="IHttpClientFactory{THttpClient,TParameter}"/>
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
            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromSeconds(this._configuration.AppSettings.Network.ConnectionLifetimeInSeconds())
            };

            var httpClient = new HttpClient(handler);

            // Set universal Request Headers
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(DefaultValues.Request.ContentType));

            // Set custom Request Headers
            for (int index = 0; index < requestHeaders.Length; index++)
            {
                httpClient.DefaultRequestHeaders.Add(requestHeaders[index].Name, requestHeaders[index].Value);
            }

            return httpClient;
        }
    }
}