// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;

namespace EventsHandler.Services.DataQuerying.Adapter
{
    /// <inheritdoc cref="IQueryContext"/>
    internal sealed class QueryContext : IQueryContext
    {
        private readonly IQueryBase _queryBase;
        private readonly IQueryKlant _queryKlant;
        private readonly IQueryZaak _queryZaak;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryContext"/> nested class.
        /// </summary>
        public QueryContext(IQueryBase queryBase, IQueryKlant queryKlant, IQueryZaak queryZaak)
        {
            // Composition
            this._queryBase = queryBase;
            this._queryKlant = queryKlant;
            this._queryZaak = queryZaak;
        }

        #region IQueryBase
        /// <inheritdoc cref="IQueryContext.SetNotification(NotificationEvent)"/>
        void IQueryContext.SetNotification(NotificationEvent notification)
            => this._queryBase.Notification = notification;

        /// <inheritdoc cref="IQueryContext.ProcessGetAsync{TModel}(HttpClientTypes, Uri, string)"/>
        async Task<TModel> IQueryContext.ProcessGetAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, string fallbackErrorMessage)
            => await this._queryBase.ProcessGetAsync<TModel>(httpsClientType, uri, fallbackErrorMessage);

        /// <inheritdoc cref="IQueryContext.ProcessPostAsync{TModel}(HttpClientTypes, Uri, HttpContent, string)"/>
        async Task<TModel> IQueryContext.ProcessPostAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, HttpContent body, string fallbackErrorMessage)
            => await this._queryBase.ProcessPostAsync<TModel>(httpsClientType, uri, body, fallbackErrorMessage);
        #endregion

        #region IQueryKlant
        /// <inheritdoc cref="IQueryContext.GetCitizenDetailsAsync(string?)"/>
        async Task<CitizenDetails> IQueryContext.GetCitizenDetailsAsync(string? bsnNumber)
        {
            // 1. Fetch BSN using "OpenZaak" Web service (if it wasn't provided already)
            bsnNumber ??= await ((IQueryContext)this).GetBsnNumberAsync();

            // 2. Fetch citizen details using "OpenKlant" Web service
            return await this._queryKlant.GetCitizenDetailsAsync(this._queryBase, bsnNumber);
        }
        #endregion

        #region IQueryZaak
        /// <inheritdoc cref="IQueryContext.GetCaseAsync()"/>
        async Task<Case> IQueryContext.GetCaseAsync()
            => await this._queryZaak.GetCaseAsync(this._queryBase);

        /// <inheritdoc cref="IQueryContext.GetCaseStatusesAsync()"/>
        async Task<CaseStatuses> IQueryContext.GetCaseStatusesAsync()
            => await this._queryZaak.GetCaseStatusesAsync(this._queryBase);

        /// <inheritdoc cref="IQueryContext.GetLastCaseStatusTypeAsync(CaseStatuses?)"/>
        async Task<CaseStatusType> IQueryContext.GetLastCaseStatusTypeAsync(CaseStatuses? statuses)
        {
            // 1. Fetch case statuses (if they weren't provided already) from "OpenZaak" Web service
            statuses ??= await ((IQueryContext)this).GetCaseStatusesAsync();

            // 2. Fetch the case status type from the last case status from "OpenZaak" Web service
            return await this._queryZaak.GetLastCaseStatusTypeAsync(this._queryBase, statuses.Value);
        }

        /// <inheritdoc cref="IQueryContext.GetBsnNumberAsync()"/>
        async Task<string> IQueryContext.GetBsnNumberAsync()
            => await this._queryZaak.GetBsnNumberAsync(this._queryBase);
        #endregion
    }
}