// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataReceiving.Enums;

namespace EventsHandler.Services.DataQuerying.Interfaces
{
    /// <summary>
    /// The nested query context (following loosely Builder pattern) operating on <see cref="NotificationEvent"/>.
    /// </summary>
    internal interface IQueryContext
    {
        /// <summary>
        /// The notification from "Notificatie API" Web service.
        /// </summary>
        internal NotificationEvent Notification { set; }

        /// <summary>
        /// Sends the <see cref="HttpMethods.Get"/> request to the specified URI and deserializes received JSON result.
        /// </summary>
        internal Task<TModel> ProcessGetAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, string fallbackErrorMessage)
            where TModel : struct, IJsonSerializable;

        /// <summary>
        /// Sends the <see cref="HttpMethods.Post"/> request to the specified URI and deserializes received JSON result.
        /// </summary>
        internal Task<TModel> ProcessPostAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, HttpContent body, string fallbackErrorMessage)
            where TModel : struct, IJsonSerializable;
    }
}