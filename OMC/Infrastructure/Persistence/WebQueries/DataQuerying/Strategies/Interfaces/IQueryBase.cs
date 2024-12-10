// © 2024, Worth Systems.

using Microsoft.AspNetCore.Http;
using System.Text.Json;
using WebQueries.DataSending.Clients.Enums;
using WebQueries.Exceptions;
using ZhvModels.Mapping.Models.Interfaces;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;

namespace WebQueries.DataQuerying.Strategies.Interfaces
{
    /// <summary>
    /// The common HTTP methods to be used for requesting data from external services.
    /// </summary>
    public interface IQueryBase
    {
        /// <summary>
        /// The notification from "OpenNotificaties" Web API service.
        /// </summary>
        internal NotificationEvent Notification { get; set; }

        /// <summary>
        /// Sends the <see cref="HttpMethods.Get"/> request to the specified URI and deserializes received JSON result.
        /// </summary>
        /// <typeparam name="TModel">The target model into which the HTTP Response (JSON) will be deserialized.</typeparam>
        /// <param name="httpClientType">The type of <see cref="HttpClient"/> to be used.</param>
        /// <param name="uri">The URI to be used during <see cref="HttpMethods.Get"/> request.</param>
        /// <param name="fallbackErrorMessage">The message to be returned in case of failed request.</param>
        /// <returns>
        ///   Deserialized HTTP Response.
        /// </returns>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal Task<TModel> ProcessGetAsync<TModel>(HttpClientTypes httpClientType, Uri uri, string fallbackErrorMessage)
            where TModel : struct, IJsonSerializable;

        /// <summary>
        /// Sends the <see cref="HttpMethods.Post"/> request to the specified URI and deserializes received JSON result.
        /// </summary>
        /// <typeparam name="TModel">The target model into which the HTTP Response (JSON) will be deserialized.</typeparam>
        /// <param name="httpClientType">The type of <see cref="HttpClient"/> to be used.</param>
        /// <param name="uri">The URI to be used during <see cref="HttpMethods.Post"/> request.</param>
        /// <param name="jsonBody">The content in JSON format to be passed with <see cref="HttpMethods.Post"/> request as HTTP Request Body.</param>
        /// <param name="fallbackErrorMessage">The message to be returned in case of failed request.</param>
        /// <returns>
        ///   Deserialized HTTP Response.
        /// </returns>
        /// <exception cref="HttpRequestException"/> or <exception cref="TelemetryException"/>
        /// <exception cref="JsonException"/>
        internal Task<TModel> ProcessPostAsync<TModel>(HttpClientTypes httpClientType, Uri uri, string jsonBody, string fallbackErrorMessage)
            where TModel : struct, IJsonSerializable;
    }
}