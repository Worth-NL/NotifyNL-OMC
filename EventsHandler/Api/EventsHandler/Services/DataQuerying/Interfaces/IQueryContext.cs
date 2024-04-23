// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
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
        internal NotificationEvent Notification { get; set; }

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
        
        /// <summary>
        /// Gets the <see cref="Case"/> from "OpenZaak" Web service.
        /// </summary>
        internal Task<Case> GetCaseAsync();
        
        /// <summary>
        /// Gets the details of a specific citizen from "OpenKlant" Web service.
        /// </summary>
        internal Task<CitizenDetails> GetCitizenDetailsAsync();

        /// <summary>
        /// Gets the status(es) of the specific <see cref="Case"/> from "OpenZaak" Web service.
        /// </summary>
        internal Task<CaseStatuses> GetCaseStatusesAsync();

        /// <summary>
        /// Gets the type of <see cref="CaseStatus"/>.
        /// </summary>
        internal Task<CaseStatusType> GetLastCaseStatusTypeAsync(CaseStatuses statuses);
    }
}