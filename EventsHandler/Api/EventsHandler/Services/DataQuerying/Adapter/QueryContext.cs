// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.Objecten;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Mapping.Models.POCOs.OpenZaak.Decision;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.Objecten.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
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
        /// <inheritdoc cref="IQueryContext.GetCaseAsync(object?)"/>
        async Task<Case> IQueryContext.GetCaseAsync(object? parameter)
            => await this._queryZaak.TryGetCaseAsync(this._queryBase, parameter);

        /// <inheritdoc cref="IQueryContext.GetCaseStatusesAsync(Uri?)"/>
        async Task<CaseStatuses> IQueryContext.GetCaseStatusesAsync(Uri? caseUri)
            => await this._queryZaak.TryGetCaseStatusesAsync(this._queryBase, caseUri);

        /// <inheritdoc cref="IQueryContext.GetLastCaseTypeAsync(CaseStatuses?)"/>
        async Task<CaseType> IQueryContext.GetLastCaseTypeAsync(CaseStatuses? caseStatuses)
        {
            // 1. Fetch case statuses (if they weren't provided already) from "OpenZaak" Web API service
            caseStatuses ??= await ((IQueryContext)this).GetCaseStatusesAsync();

            // 2. Fetch the case status type from the last case status from "OpenZaak" Web API service
            return await this._queryZaak.GetLastCaseTypeAsync(this._queryBase, caseStatuses.Value);
        }

        /// <inheritdoc cref="IQueryContext.GetMainObjectAsync()"/>
        async Task<MainObject> IQueryContext.GetMainObjectAsync()
            => await this._queryZaak.GetMainObjectAsync(this._queryBase);

        /// <inheritdoc cref="IQueryContext.GetDecisionResourceAsync(Uri?)"/>
        async Task<DecisionResource> IQueryContext.GetDecisionResourceAsync(Uri? resourceUri)
            => await this._queryZaak.TryGetDecisionResourceAsync(this._queryBase, resourceUri);

        /// <inheritdoc cref="IQueryContext.GetInfoObjectAsync(object?)"/>
        async Task<InfoObject> IQueryContext.GetInfoObjectAsync(object? parameter)
            => await this._queryZaak.TryGetInfoObjectAsync(this._queryBase, parameter);

        /// <inheritdoc cref="IQueryContext.GetDecisionAsync(DecisionResource?)"/>
        async Task<Decision> IQueryContext.GetDecisionAsync(DecisionResource? decisionResource)
            => await this._queryZaak.TryGetDecisionAsync(this._queryBase, decisionResource);

        /// <inheritdoc cref="IQueryContext.GetDocumentsAsync(DecisionResource?)"/>
        async Task<Documents> IQueryContext.GetDocumentsAsync(DecisionResource? decisionResource)
            => await this._queryZaak.TryGetDocumentsAsync(this._queryBase, decisionResource);

        /// <inheritdoc cref="IQueryContext.GetDecisionTypeAsync(Decision?)"/>
        async Task<DecisionType> IQueryContext.GetDecisionTypeAsync(Decision? decision)
            => await this._queryZaak.TryGetDecisionTypeAsync(this._queryBase, decision);

        /// <inheritdoc cref="IQueryContext.SendFeedbackToOpenZaakAsync(HttpContent)"/>
        async Task<string> IQueryContext.SendFeedbackToOpenZaakAsync(HttpContent body)
            => await this._queryZaak.SendFeedbackAsync(this._networkService, body);

        /// <inheritdoc cref="IQueryContext.GetBsnNumberAsync(Uri)"/>
        async Task<string> IQueryContext.GetBsnNumberAsync(Uri caseTypeUri)
        {
            // 1. Fetch the case roles from "OpenZaak"
            // 2. Determine the citizen data from the case roles
            // 3. Return BSN from the citizen data
            return await this._queryZaak.GetBsnNumberAsync(this._queryBase, caseTypeUri);
        }

        /// <inheritdoc cref="IQueryContext.GetCaseTypeUriAsync(Uri?)"/>
        async Task<Uri> IQueryContext.GetCaseTypeUriAsync(Uri? caseUri)
        {
            // 1a. Gets the case type URI directly from the initial notification
            // 1b. Use the provided case URI to retrieve the case type URI from CaseDetails
            return await this._queryZaak.TryGetCaseTypeUriAsync(this._queryBase, caseUri);
        }
        #endregion

        #region IQueryKlant
        /// <inheritdoc cref="IQueryContext.GetPartyDataAsync(string?)"/>
        async Task<CommonPartyData> IQueryContext.GetPartyDataAsync(string? bsnNumber)
        {
            // 1. Fetch BSN using "OpenZaak" Web API service (if it wasn't provided already)
            bsnNumber ??= await ((IQueryContext)this).GetBsnNumberAsync(
                this._queryBase.Notification.MainObjectUri);  // In Cases scenarios the desired case type URI is located here

            // 2. Fetch citizen details using "OpenKlant" Web API service
            return await this._queryKlant.TryGetPartyDataAsync(this._queryBase, bsnNumber);
        }

        /// <inheritdoc cref="IQueryContext.SendFeedbackToOpenKlantAsync(HttpContent)"/>
        async Task<ContactMoment> IQueryContext.SendFeedbackToOpenKlantAsync(HttpContent body)
            => await this._queryKlant.SendFeedbackAsync(this._queryBase, body);

        /// <inheritdoc cref="IQueryContext.LinkToSubjectObjectAsync(HttpContent)"/>
        async Task<string> IQueryContext.LinkToSubjectObjectAsync(HttpContent body)
            => await OpenKlant.v2.QueryKlant.LinkToSubjectObjectAsync(this._networkService, this._queryKlant.GetDomain(), body);
        #endregion

        #region IQueryObjecten
        /// <inheritdoc cref="IQueryContext.GetTaskAsync()"/>
        Task<TaskObject> IQueryContext.GetTaskAsync()
            => this._queryObjecten.GetTaskAsync(this._queryBase);
        #endregion
    }
}