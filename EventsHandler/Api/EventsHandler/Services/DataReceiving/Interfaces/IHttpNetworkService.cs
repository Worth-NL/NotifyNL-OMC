// © 2023, Worth Systems.

using EventsHandler.Services.DataReceiving.Enums;

namespace EventsHandler.Services.DataReceiving.Interfaces
{
    /// <summary>
    /// The service to retrieve data from different services supplying them to business logic.
    /// </summary>
    public interface IHttpNetworkService
    {
        /// <summary>
        /// Sends request to the given Web API service using a specific <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="httpClientType">The type of the specialized <see cref="HttpClient"/>.</param>
        /// <returns>
        ///   The <see langword="string"/> JSON response from the Web API service.
        /// </returns>
        /// <param name="uri">The URI to be used with <see cref="HttpMethod.Get"/> request.</param>
        internal Task<(bool Success, string JsonResponse)> GetAsync(HttpClientTypes httpClientType, Uri uri);

        /// <summary>
        /// Posts request to the given Web API service using a specific <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="httpClientType">The type of the specialized <see cref="HttpClient"/>.</param>
        /// <returns>
        ///   The <see langword="string"/> JSON response from the Web API service.
        /// </returns>
        /// <param name="uri">The URI to be used with <see cref="HttpMethod.Post"/> request.</param>
        /// <param name="body">The HTTP content to be passed with <see cref="HttpMethod.Post"/> request.</param>
        internal Task<(bool Success, string JsonResponse)> PostAsync(HttpClientTypes httpClientType, Uri uri, HttpContent body);
    }
}