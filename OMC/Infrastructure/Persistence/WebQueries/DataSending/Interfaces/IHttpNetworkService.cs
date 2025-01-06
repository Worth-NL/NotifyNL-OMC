// © 2023, Worth Systems.

using Microsoft.AspNetCore.Http;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataSending.Clients.Enums;

namespace WebQueries.DataSending.Interfaces
{
    /// <summary>
    /// The service defining basic HTTP Requests contracts (e.g., GET, POST).
    /// </summary>
    public interface IHttpNetworkService
    {
        /// <summary>
        /// Sends request to the given Web API service using a specific <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="httpClientType">The type of the specialized <see cref="HttpClient"/>.</param>
        /// <param name="uri">The URI to be used with <see cref="HttpMethod.Get"/> request.</param>
        /// <returns>
        ///   The <see langword="string"/> JSON response from the Web API service.
        /// </returns>
        internal Task<HttpRequestResponse> GetAsync(HttpClientTypes httpClientType, Uri uri);

        /// <summary>
        /// Posts request to the given Web API service using a specific <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="httpClientType">The type of the specialized <see cref="HttpClient"/>.</param>
        /// <param name="uri">The URI to be used with <see cref="HttpMethod.Post"/> request.</param>
        /// <param name="jsonBody">The content in JSON format to be passed with <see cref="HttpMethods.Post"/> request as HTTP Request Body.</param>
        /// <returns>
        ///   The <see langword="string"/> JSON response from the Web API service.
        /// </returns>
        internal Task<HttpRequestResponse> PostAsync(HttpClientTypes httpClientType, Uri uri, string jsonBody);
    }
}