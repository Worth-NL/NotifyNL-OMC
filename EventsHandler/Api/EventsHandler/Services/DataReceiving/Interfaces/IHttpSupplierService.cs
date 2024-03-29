﻿// © 2023, Worth Systems.

using EventsHandler.Configuration;
using EventsHandler.Services.DataReceiving.Enums;
using System.Collections.Concurrent;

namespace EventsHandler.Services.DataReceiving.Interfaces
{
    /// <summary>
    /// The service to retrieve data from different services supplying them to business logic.
    /// </summary>
    public interface IHttpSupplierService
    {
        /// <inheritdoc cref="WebApiConfiguration"/>
        internal WebApiConfiguration Configuration { get; }

        /// <summary>
        /// Gets the internal <see cref="HttpClient"/>s used for specific business purposes.
        /// </summary>
        internal ConcurrentDictionary<HttpClientTypes, HttpClient> HttpClients { get; }

        /// <summary>
        /// Sends request to the given Web service using a specific <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="httpClientType">The type of the specialized <see cref="HttpClient"/>.</param>
        /// <param name="organizationId">The organization identifier.</param>
        /// <returns>
        ///   The <see langword="string"/> JSON response from the Web service.
        /// </returns>
        /// <param name="uri">The URI to be used with <see cref="HttpMethod.Get"/> request.</param>
        internal Task<(bool Success, string JsonResponse)> GetAsync(HttpClientTypes httpClientType, string organizationId, Uri uri);

        /// <summary>
        /// Posts request to the given Web service using a specific <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="httpClientType">The type of the specialized <see cref="HttpClient"/>.</param>
        /// <param name="organizationId">The organization identifier.</param>
        /// <returns>
        ///   The <see langword="string"/> JSON response from the Web service.
        /// </returns>
        /// <param name="uri">The URI to be used with <see cref="HttpMethod.Post"/> request.</param>
        /// <param name="body">The HTTP content to be passed with <see cref="HttpMethod.Post"/> request.</param>
        internal Task<(bool Success, string JsonResponse)> PostAsync(HttpClientTypes httpClientType, string organizationId, Uri uri, HttpContent body);
    }
}