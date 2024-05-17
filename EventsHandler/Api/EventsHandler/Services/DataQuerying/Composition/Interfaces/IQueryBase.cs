// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataReceiving.Enums;

namespace EventsHandler.Services.DataQuerying.Composition.Interfaces
{
    /// <summary>
    /// The common HTTP methods to be used for requesting data from external services.
    /// </summary>
    internal interface IQueryBase
    {
        /// <summary>
        /// The notification from "Notificatie API" Web service.
        /// </summary>
        internal NotificationEvent Notification { get; set; }
        
        /// <summary>
        /// Sends the <see cref="HttpMethods.Get"/> request to the specified URI and deserializes received JSON result.
        /// </summary>
        /// <typeparam name="TModel">The target model into which the HTTP Response (JSON) will be deserialized.</typeparam>
        /// <param name="httpsClientType">The type of <see cref="HttpClient"/> to be used.</param>
        /// <param name="uri">The URI to be used during <see cref="HttpMethods.Get"/> request.</param>
        /// <param name="fallbackErrorMessage">The message to be returned in case of failed request.</param>
        /// <returns>
        ///   Deserialized HTTP Response.
        /// </returns>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        internal Task<TModel> ProcessGetAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, string fallbackErrorMessage)
            where TModel : struct, IJsonSerializable;

        /// <summary>
        /// Sends the <see cref="HttpMethods.Post"/> request to the specified URI and deserializes received JSON result.
        /// </summary>
        /// <typeparam name="TModel">The target model into which the HTTP Response (JSON) will be deserialized.</typeparam>
        /// <param name="httpsClientType">The type of <see cref="HttpClient"/> to be used.</param>
        /// <param name="uri">The URI to be used during <see cref="HttpMethods.Post"/> request.</param>
        /// <param name="body">The HTTP Body to be sent along with <see cref="HttpMethods.Post"/> request.</param>
        /// <param name="fallbackErrorMessage">The message to be returned in case of failed request.</param>
        /// <returns>
        ///   Deserialized HTTP Response.
        /// </returns>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        internal Task<TModel> ProcessPostAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, HttpContent body, string fallbackErrorMessage)
            where TModel : struct, IJsonSerializable;
    }
}