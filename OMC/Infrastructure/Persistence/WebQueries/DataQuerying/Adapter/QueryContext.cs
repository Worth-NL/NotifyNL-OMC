// © 2024, Worth Systems.

using Common.Extensions;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.Besluiten.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.Objecten.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.ObjectTypen.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenKlant.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenZaak.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.Properties;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.Objecten.Message;
using ZhvModels.Mapping.Models.POCOs.Objecten.Task;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;
using ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision;

namespace WebQueries.DataQuerying.Adapter
{
    /// <inheritdoc cref="IQueryContext"/>
    public sealed class QueryContext : IQueryContext
    {
        private readonly IHttpNetworkService _networkService;  // Universal raw HTTP methods

        private readonly IQueryBase _queryBase;                // Common class for API microservices

        private readonly IQueryZaak _queryZaak;                // Case API microservice
        private readonly IQueryKlant _queryKlant;              // Client API microservice
        private readonly IQueryBesluiten _queryBesluiten;      // Decision API microservice
        private readonly IQueryObjecten _queryObjecten;        // Object API microservice
        private readonly IQueryObjectTypen _queryObjectTypen;  // ObjectType API microservice

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryContext"/> nested class.
        /// </summary>
        public QueryContext(
            IHttpNetworkService networkService,
            IQueryBase queryBase,
            IQueryZaak queryZaak,
            IQueryKlant queryKlant,
            IQueryBesluiten queryBesluiten,
            IQueryObjecten queryObjecten,
            IQueryObjectTypen queryObjectTypen)  // Dependency Injection (DI)
        {
            // Composition
            this._networkService = networkService;
            this._queryBase = queryBase;
            this._queryZaak = queryZaak;
            this._queryKlant = queryKlant;
            this._queryBesluiten = queryBesluiten;
            this._queryObjecten = queryObjecten;
            this._queryObjectTypen = queryObjectTypen;
        }

        #region IQueryBase
        /// <inheritdoc cref="IQueryContext.SetNotification(NotificationEvent)"/>
        void IQueryContext.SetNotification(NotificationEvent notification)
        {
            _queryBase.Notification = notification;
        }
        #endregion

        #region IQueryZaak
        /// <inheritdoc cref="IQueryContext.GetZaakHealthCheckAsync"/>
        async Task<HttpRequestResponse> IQueryContext.GetZaakHealthCheckAsync()
            => await _queryZaak.GetHealthCheckAsync(_networkService);

        /// <inheritdoc cref="IQueryContext.GetCaseAsync(Uri?)"/>
        async Task<Case> IQueryContext.GetCaseAsync(Uri? caseUri)
            => await _queryZaak.TryGetCaseAsync(_queryBase, caseUri);

        /// <inheritdoc cref="IQueryContext.GetCaseStatusesAsync(Uri?)"/>
        async Task<CaseStatuses> IQueryContext.GetCaseStatusesAsync(Uri? caseUri)
            => await _queryZaak.TryGetCaseStatusesAsync(_queryBase, caseUri);

        /// <inheritdoc cref="IQueryContext.GetLastCaseTypeAsync(CaseStatuses?)"/>
        async Task<CaseType> IQueryContext.GetLastCaseTypeAsync(CaseStatuses? caseStatuses)
        {
            // 1. Fetch case statuses (if they weren't provided already) from "OpenZaak" Web API service
            caseStatuses ??= await ((IQueryContext)this).GetCaseStatusesAsync();

            // 2. Fetch the case status type from the last case status from "OpenZaak" Web API service
            return await _queryZaak.GetLastCaseTypeAsync(_queryBase, caseStatuses.Value);
        }

        /// <inheritdoc cref="IQueryContext.GetBsnNumberAsync(Uri)"/>
        async Task<string> IQueryContext.GetBsnNumberAsync(Uri caseUri)
        {
            // 1. Fetch the case roles from "OpenZaak"
            // 2. Determine the citizen data from the case roles
            // 3. Return BSN from the citizen data
            return await _queryZaak.GetBsnNumberAsync(_queryBase, caseUri);
        }

        /// <inheritdoc cref="IQueryContext.GetCaseTypeUriAsync(Uri?)"/>
        async Task<Uri> IQueryContext.GetCaseTypeUriAsync(Uri? caseUri)
        {
            // 1a. Gets the case type URI directly from the initial notification
            // 1b. Use the provided case URI to retrieve the case type URI from CaseDetails
            return await _queryZaak.TryGetCaseTypeUriAsync(_queryBase, caseUri);
        }
        #endregion

        #region IQueryKlant
        /// <inheritdoc cref="IQueryContext.GetKlantHealthCheckAsync"/>
        async Task<HttpRequestResponse> IQueryContext.GetKlantHealthCheckAsync()
            => await _queryKlant.GetHealthCheckAsync(_networkService);

        /// <inheritdoc cref="IQueryContext.GetPartyDataAsync(Uri?, string?)"/>
        async Task<CommonPartyData> IQueryContext.GetPartyDataAsync(Uri? caseUri, string? bsnNumber)
        {
            // Case #1: Case URI was not provided, which means for 100% the citizen data are requested
            if (caseUri.IsNullOrDefault())
            {
                return bsnNumber.IsNullOrEmpty()  // But wouldn't be able to be retrieved if the BSN number is missing
                    ? throw new ArgumentException(QueryResources.Querying_ERROR_Internal_MissingBsnNumber)
                    : await _queryKlant.TryGetPartyDataAsync(_queryBase, bsnNumber);
            }

            CaseRole caseRole = await _queryZaak.GetCaseRoleAsync(_queryBase, caseUri);

            // Case #2: Involved Party URI is missing => getting citizen data by its BSN number will be attempted
            if (caseRole.InvolvedPartyUri.IsNullOrDefault())
            {
                // 1. Fetch BSN using "OpenZaak" Web API service (if it wasn't provided already)
                bsnNumber ??= await ((IQueryContext)this).GetBsnNumberAsync(caseUri);

                // 2. Fetch citizen data using "OpenKlant" Web API service
                return await _queryKlant.TryGetPartyDataAsync(_queryBase, bsnNumber);
            }

            // Case #3: Since Involved Party URI is present => getting organization data will be attempted
            return await _queryKlant.TryGetPartyDataAsync(_queryBase, caseRole.InvolvedPartyUri);
        }

        /// <inheritdoc cref="IQueryContext.CreateContactMomentAsync(string)"/>
        async Task<ContactMoment> IQueryContext.CreateContactMomentAsync(string jsonBody)
            => await _queryKlant.CreateContactMomentAsync(_queryBase, jsonBody);

        /// <inheritdoc cref="IQueryContext.LinkCaseToContactMomentAsync(string)"/>
        async Task<HttpRequestResponse> IQueryContext.LinkCaseToContactMomentAsync(string jsonBody)
            => await _queryKlant.LinkCaseToContactMomentAsync(_networkService, jsonBody);

        /// <inheritdoc cref="IQueryContext.LinkPartyToContactMomentAsync"/>
        async Task<HttpRequestResponse> IQueryContext.LinkPartyToContactMomentAsync(string jsonBody)
            => await _queryKlant.LinkPartyToContactMomentAsync(_networkService, jsonBody);
        #endregion

        #region IQueryBesluiten
        /// <inheritdoc cref="IQueryContext.GetBesluitenHealthCheckAsync"/>
        async Task<HttpRequestResponse> IQueryContext.GetBesluitenHealthCheckAsync()
            => await _queryBesluiten.GetHealthCheckAsync(_networkService);

        /// <inheritdoc cref="IQueryContext.GetDecisionResourceAsync(Uri?)"/>
        async Task<DecisionResource> IQueryContext.GetDecisionResourceAsync(Uri? resourceUri)
            => await _queryBesluiten.TryGetDecisionResourceAsync(_queryBase, resourceUri);

        /// <inheritdoc cref="IQueryContext.GetInfoObjectAsync(object?)"/>
        async Task<InfoObject> IQueryContext.GetInfoObjectAsync(object? parameter)
            => await _queryBesluiten.TryGetInfoObjectAsync(_queryBase, parameter);

        /// <inheritdoc cref="IQueryContext.GetDecisionAsync(DecisionResource?)"/>
        async Task<Decision> IQueryContext.GetDecisionAsync(DecisionResource? decisionResource)
            => await _queryBesluiten.TryGetDecisionAsync(_queryBase, decisionResource);

        /// <inheritdoc cref="IQueryContext.GetDocumentsAsync(DecisionResource?)"/>
        async Task<Documents> IQueryContext.GetDocumentsAsync(DecisionResource? decisionResource)
            => await _queryBesluiten.TryGetDocumentsAsync(_queryBase, decisionResource);

        /// <inheritdoc cref="IQueryContext.GetDecisionTypeAsync(Decision?)"/>
        async Task<DecisionType> IQueryContext.GetDecisionTypeAsync(Decision? decision)
            => await _queryBesluiten.TryGetDecisionTypeAsync(_queryBase, decision);
        #endregion

        #region IQueryObjecten
        /// <inheritdoc cref="IQueryContext.GetObjectenHealthCheckAsync"/>
        async Task<HttpRequestResponse> IQueryContext.GetObjectenHealthCheckAsync()
            => await _queryObjecten.GetHealthCheckAsync(_networkService);

        /// <inheritdoc cref="IQueryContext.GetTaskAsync()"/>
        Task<CommonTaskData> IQueryContext.GetTaskAsync()
            => _queryObjecten.GetTaskAsync(_queryBase);

        /// <inheritdoc cref="IQueryContext.GetMessageAsync()"/>
        Task<MessageObject> IQueryContext.GetMessageAsync()
            => _queryObjecten.GetMessageAsync(_queryBase);

        /// <inheritdoc cref="IQueryContext.CreateObjectAsync(string)"/>
        async Task<HttpRequestResponse> IQueryContext.CreateObjectAsync(string objectJsonBody)
            => await _queryObjecten.CreateObjectAsync(_networkService, objectJsonBody);
        #endregion

        #region IQueryObjectTypen
        /// <inheritdoc cref="IQueryContext.GetObjectTypenHealthCheckAsync"/>
        async Task<HttpRequestResponse> IQueryContext.GetObjectTypenHealthCheckAsync()
            => await _queryObjectTypen.GetHealthCheckAsync(_networkService);

        /// <inheritdoc cref="IQueryContext.PrepareObjectJsonBody(string)"/>
        string IQueryContext.PrepareObjectJsonBody(string dataJson)
            => _queryObjectTypen.PrepareObjectJsonBody(dataJson);
        #endregion
    }
}