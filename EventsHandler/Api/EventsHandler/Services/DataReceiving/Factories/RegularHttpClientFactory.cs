// © 2023, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Services.DataReceiving.Factories.Interfaces;
using System.Net.Http.Headers;

namespace EventsHandler.Services.DataReceiving.Factories
{
    /// <inheritdoc cref="IHttpClientFactory{THttpClient,TParameter}"/>
    internal sealed class RegularHttpClientFactory : IHttpClientFactory<HttpClient, (string Name, string Value)[]>
    {
        /// <inheritdoc cref="IHttpClientFactory{THttpClient,TParameters}.GetHttpClient(TParameters)"/>
        HttpClient IHttpClientFactory<HttpClient, (string Name, string Value)[]>.GetHttpClient((string Name, string Value)[] requestHeaders)
        {
            var httpClient = new HttpClient();

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