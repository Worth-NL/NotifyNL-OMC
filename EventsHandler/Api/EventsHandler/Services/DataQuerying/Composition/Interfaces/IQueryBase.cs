// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataReceiving.Enums;

namespace EventsHandler.Services.DataQuerying.Composition.Interfaces
{
    /// <summary>
    /// Common HTTP methods to be used for requesting data from external services.
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
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        internal Task<TModel> ProcessGetAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, string fallbackErrorMessage)
            where TModel : struct, IJsonSerializable;

        /// <summary>
        /// Sends the <see cref="HttpMethods.Post"/> request to the specified URI and deserializes received JSON result.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        internal Task<TModel> ProcessPostAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, HttpContent body, string fallbackErrorMessage)
            where TModel : struct, IJsonSerializable;
    }
}