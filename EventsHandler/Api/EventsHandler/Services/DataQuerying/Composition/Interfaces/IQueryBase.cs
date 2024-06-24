// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Exceptions;
using EventsHandler.Services.DataReceiving.Enums;
using System.Text.Json;

namespace EventsHandler.Services.DataQuerying.Composition.Interfaces
{
    /// <summary>
    /// The common HTTP methods to be used for requesting data from external services.
    /// </summary>
    internal interface IQueryBase
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
        /// <param name="body">The HTTP Body to be sent along with <see cref="HttpMethods.Post"/> request.</param>
        /// <param name="fallbackErrorMessage">The message to be returned in case of failed request.</param>
        /// <returns>
        ///   Deserialized HTTP Response.
        /// </returns>
        /// <exception cref="HttpRequestException"/> or <exception cref="TelemetryException"/>
        /// <exception cref="JsonException"/>
        internal Task<TModel> ProcessPostAsync<TModel>(HttpClientTypes httpClientType, Uri uri, HttpContent body, string fallbackErrorMessage)
            where TModel : struct, IJsonSerializable;
    }
}