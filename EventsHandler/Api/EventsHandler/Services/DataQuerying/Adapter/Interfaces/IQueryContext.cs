// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;

namespace EventsHandler.Services.DataQuerying.Adapter.Interfaces
{
    /// <summary>
    /// The adapter combining functionalities from other data-querying-related interfaces.
    /// </summary>
    internal interface IQueryContext
    {
        #region IQueryBase        
        /// <summary>
        /// Sets the <see cref="NotificationEvent"/> in <see cref="IQueryBase.Notification"/>.
        /// </summary>
        internal void SetNotification(NotificationEvent notification);

        /// <inheritdoc cref="IQueryBase.ProcessPostAsync{TModel}(HttpClientTypes, Uri, HttpContent, string)"/>
        internal Task<TModel> ProcessPostAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, HttpContent body, string fallbackErrorMessage)
            where TModel : struct, IJsonSerializable;
        #endregion

        #region IQueryKlant
        /// <inheritdoc cref="IQueryKlant.GetCitizenDetailsAsync(IQueryBase, string)"/>
        internal Task<CitizenDetails> GetCitizenDetailsAsync();
        #endregion

        /// <summary>
        /// Gets the <see cref="Case"/> from "OpenZaak" Web service.
        /// </summary>
        internal Task<Case> GetCaseAsync();

        /// <summary>
        /// Gets the status(es) of the specific <see cref="Case"/> from "OpenZaak" Web service.
        /// </summary>
        internal Task<CaseStatuses> GetCaseStatusesAsync();

        /// <summary>
        /// Gets the type of <see cref="CaseStatus"/> from "OpenZaak" Web service.
        /// </summary>
        internal Task<CaseStatusType> GetLastCaseStatusTypeAsync(CaseStatuses statuses);
    }
}