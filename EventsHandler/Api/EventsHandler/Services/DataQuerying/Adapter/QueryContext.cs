// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Exceptions;
using EventsHandler.Properties;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.Objecten.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces;
using EventsHandler.Services.DataReceiving.Interfaces;
using OpenKlant = EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant;

namespace EventsHandler.Services.DataQuerying.Adapter
{
    /// <inheritdoc cref="IQueryContext"/>
    internal sealed class QueryContext : IQueryContext
    {
        private readonly IHttpNetworkService _networkService;
        private readonly IQueryBase _queryBase;
        private readonly IQueryKlant _queryKlant;
        private readonly IQueryZaak _queryZaak;
        private readonly IQueryObjecten _queryObjecten;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryContext"/> nested class.
        /// </summary>
        public QueryContext(
            IHttpNetworkService networkService,
            IQueryBase queryBase,
            IQueryKlant queryKlant,
            IQueryZaak queryZaak,
            IQueryObjecten queryObjecten)
        {
            // Composition
            this._networkService = networkService;
            this._queryBase = queryBase;
            this._queryKlant = queryKlant;
            this._queryZaak = queryZaak;
            this._queryObjecten = queryObjecten;
        }

        #region IQueryBase
        /// <inheritdoc cref="IQueryContext.SetNotification(NotificationEvent)"/>
        void IQueryContext.SetNotification(NotificationEvent notification)
        {
            this._queryBase.Notification = notification;
        }
        #endregion

        #region IQueryZaak
        /// <inheritdoc cref="IQueryContext.GetCaseAsync()"/>
        async Task<Case> IQueryContext.GetCaseAsync()
            => await this._queryZaak.GetCaseAsync(this._queryBase);

        /// <inheritdoc cref="IQueryContext.GetCaseAsync(Uri?)"/>
        async Task<Case> IQueryContext.GetCaseAsync(Uri? caseTypeUri)
            => await this._queryZaak.GetCaseAsync(this._queryBase, caseTypeUri);

        /// <inheritdoc cref="IQueryContext.GetCaseStatusesAsync()"/>
        async Task<CaseStatuses> IQueryContext.GetCaseStatusesAsync()
            => await this._queryZaak.GetCaseStatusesAsync(this._queryBase);

        /// <inheritdoc cref="IQueryContext.GetLastCaseStatusTypeAsync(CaseStatuses?)"/>
        async Task<CaseStatusType> IQueryContext.GetLastCaseStatusTypeAsync(CaseStatuses? statuses)
        {
            // 1. Fetch case statuses (if they weren't provided already) from "OpenZaak" Web API service
            statuses ??= await ((IQueryContext)this).GetCaseStatusesAsync();

            // 2. Fetch the case status type from the last case status from "OpenZaak" Web API service
            CaseStatusType statusType = await this._queryZaak.GetLastCaseStatusTypeAsync(this._queryBase, statuses.Value);

            return statusType.IsNotificationExpected
                ? statusType
                : throw new AbortedNotifyingException(Resources.Processing_ABORT_DoNotSendNotification_CaseStatusType);
        }

        /// <inheritdoc cref="IQueryContext.GetBsnNumberAsync()"/>
        async Task<string> IQueryContext.GetBsnNumberAsync()
        {
            // 1. MainObject from the NotificationEvent is case type URI
            // 2. Fetch case roles from "OpenZaak"
            // 3. Determine citizen data from case roles
            // 4. Return BSN from citizen data
            return await this._queryZaak.GetBsnNumberAsync(this._queryBase);
        }

        /// <inheritdoc cref="IQueryContext.GetBsnNumberAsync()"/>
        async Task<string> IQueryContext.GetBsnNumberAsync(Uri caseTypeUri)
        {
            // 1. Pass case type URI from outside (MainObject is a different one in this situation)
            // 2. Fetch case roles from "OpenZaak"
            // 3. Determine citizen data from case roles
            // 4. Return BSN from citizen data
            return await this._queryZaak.GetBsnNumberAsync(this._queryBase, caseTypeUri);
        }

        /// <inheritdoc cref="IQueryContext.GetMainObjectAsync()"/>
        async Task<MainObject> IQueryContext.GetMainObjectAsync()
            => await this._queryZaak.GetMainObjectAsync(this._queryBase);

        /// <inheritdoc cref="IQueryContext.GetDecisionAsync()"/>
        async Task<Decision> IQueryContext.GetDecisionAsync()
            => await this._queryZaak.GetDecisionAsync(this._queryBase);

        /// <inheritdoc cref="IQueryContext.SendFeedbackToOpenZaakAsync(HttpContent)"/>
        async Task<string> IQueryContext.SendFeedbackToOpenZaakAsync(HttpContent body)
        {
            return await this._queryZaak.SendFeedbackAsync(this._networkService, body);
        }
        #endregion

        #region IQueryKlant
        /// <inheritdoc cref="IQueryContext.GetPartyDataAsync(string?)"/>
        async Task<CommonPartyData> IQueryContext.GetPartyDataAsync(string? bsnNumber)
        {
            // 1. Fetch BSN using "OpenZaak" Web API service (if it wasn't provided already)
            bsnNumber ??= await ((IQueryContext)this).GetBsnNumberAsync();

            // 2. Fetch citizen details using "OpenKlant" Web API service
            return await this._queryKlant.GetPartyDataAsync(this._queryBase, bsnNumber);
        }

        /// <inheritdoc cref="IQueryContext.SendFeedbackToOpenKlantAsync(HttpContent)"/>
        async Task<ContactMoment> IQueryContext.SendFeedbackToOpenKlantAsync(HttpContent body)
        {
            return await this._queryKlant.SendFeedbackAsync(this._queryBase, body);
        }

        /// <inheritdoc cref="IQueryContext.LinkToSubjectObjectAsync(HttpContent)"/>
        async Task<string> IQueryContext.LinkToSubjectObjectAsync(HttpContent body)
        {
            return await OpenKlant.v2.QueryKlant.LinkToSubjectObjectAsync(
                this._networkService, this._queryKlant.GetSpecificOpenKlantDomain(), body);
        }
        #endregion
    }
}