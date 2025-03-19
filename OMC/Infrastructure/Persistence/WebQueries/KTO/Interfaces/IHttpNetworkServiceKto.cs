// © 2025, Worth Systems.

using Microsoft.AspNetCore.Http;
using WebQueries.DataQuerying.Models.Responses;

namespace WebQueries.KTO.Interfaces
{
    /// <summary>
    /// The service defining basic HTTP Requests contracts (e.g., GET, POST).
    /// </summary>
    public interface IHttpNetworkServiceKto
    {
        /// <summary>
        /// Sends request to the given Web API service using a specific <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="uri">The URI to be used with <see cref="HttpMethod.Get"/> request.</param>
        /// <returns>
        ///   The <see langword="string"/> JSON response from the Web API service.
        /// </returns>
        internal Task<HttpRequestResponse> GetAsync(Uri uri);

        /// <summary>
        /// Posts request to the given Web API service using a specific <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="jsonBody">The content in JSON format to be passed with <see cref="HttpMethods.Post"/> request as HTTP Request Body.</param>
        /// <returns>
        ///   The <see langword="string"/> JSON response from the Web API service.
        /// </returns>
        internal Task<HttpRequestResponse> PostAsync(string jsonBody);
    }
}