// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;

namespace EventsHandler.Services.DataQuerying.Adapter.Interfaces
{
    /// <summary>
    /// The adapter combining and adjusting functionalities from other data querying services.
    /// </summary>
    /// <remarks>
    ///   This interface is modifying signatures of methods from related services
    ///   (<see cref="IQueryBase"/>, <see cref="IQueryKlant"/>, <see cref="IQueryZaak"/>)
    ///   to hide some dependencies inside the <see cref="IQueryContext"/> implementation,
    ///   make the usage of these methods easier, and base on the injected/setup context.
    /// </remarks>
    /// <seealso cref="IQueryBase"/>
    /// <seealso cref="IQueryKlant"/>
    /// <seealso cref="IQueryZaak"/>
    internal interface IQueryContext
    {
        #region IQueryBase
        /// <summary>
        /// Sets the <see cref="NotificationEvent"/> in <see cref="IQueryBase.Notification"/>.
        /// </summary>
        internal void SetNotification(NotificationEvent notification);

        /// <inheritdoc cref="IQueryBase.ProcessGetAsync{TModel}(HttpClientTypes, Uri, string)"/>
        internal Task<TModel> ProcessGetAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, string fallbackErrorMessage)
            where TModel : struct, IJsonSerializable;

        /// <inheritdoc cref="IQueryBase.ProcessPostAsync{TModel}(HttpClientTypes, Uri, HttpContent, string)"/>
        internal Task<TModel> ProcessPostAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, HttpContent body, string fallbackErrorMessage)
            where TModel : struct, IJsonSerializable;
        #endregion

        #region IQueryKlant
        /// <inheritdoc cref="IQueryKlant.GetPartyDataAsync(IQueryBase, string)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing BSN number first, but it produces an additional
        ///   overhead since the missing BSN will be queried internally anyway from "OpenZaak" Web API service.
        /// </remarks>
        internal Task<CommonPartyData> GetPartyDataAsync(string? bsnNumber = null);
        #endregion

        #region IQueryZaak
        /// <inheritdoc cref="IQueryZaak.GetCaseAsync(IQueryBase)"/>
        internal Task<Case> GetCaseAsync();

        /// <inheritdoc cref="IQueryZaak.GetCaseStatusesAsync(IQueryBase)"/>
        internal Task<CaseStatuses> GetCaseStatusesAsync();

        /// <inheritdoc cref="IQueryZaak.GetLastCaseStatusTypeAsync(IQueryBase, CaseStatuses)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing <see cref="CaseStatus"/>es, but it produces an additional
        ///   overhead since the missing statuses will be queried internally anyway from "OpenZaak" Web API service.
        /// </remarks>
        internal Task<CaseStatusType> GetLastCaseStatusTypeAsync(CaseStatuses? statuses = null);

        /// <inheritdoc cref="IQueryZaak.GetBsnNumberAsync(IQueryBase)"/>
        internal Task<string> GetBsnNumberAsync();
        #endregion
    }
}