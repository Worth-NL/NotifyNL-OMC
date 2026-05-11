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
using WebQueries.DataQuerying.Strategies.Queries.OpenVtb.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.KTO.Interfaces;
using WebQueries.Properties;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Models.POCOs.Berichten;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.Objecten.KTO;
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
        private readonly IHttpNetworkService _networkService;
        private readonly IHttpNetworkServiceKto _networkServiceKto;

        private readonly IQueryBase _queryBase;

        private readonly IQueryZaak _queryZaak;
        private readonly IQueryKlant _queryKlant;
        private readonly IQueryBesluiten _queryBesluiten;
        private readonly IQueryObjecten _queryObjecten;
        private readonly IQueryObjectTypen _queryObjectTypen;
        private readonly IQueryVtb _queryVtb;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryContext"/> nested class.
        /// </summary>
        public QueryContext(
            IHttpNetworkService networkService,
            IHttpNetworkServiceKto networkServiceKto,
            IQueryBase queryBase,
            IQueryZaak queryZaak,
            IQueryKlant queryKlant,
            IQueryBesluiten queryBesluiten,
            IQueryObjecten queryObjecten,
            IQueryObjectTypen queryObjectTypen,
            IQueryVtb queryVtb)
        {
            this._networkService = networkService;
            this._networkServiceKto = networkServiceKto;
            this._queryBase = queryBase;
            this._queryZaak = queryZaak;
            this._queryKlant = queryKlant;
            this._queryBesluiten = queryBesluiten;
            this._queryObjecten = queryObjecten;
            this._queryObjectTypen = queryObjectTypen;
            this._queryVtb = queryVtb;
        }

        #region IQueryBase
        /// <inheritdoc cref="IQueryContext.SetNotification(NotificationEvent)"/>
        void IQueryContext.SetNotification(NotificationEvent notification)
        {
            this._queryBase.Notification = notification;
        }
        #endregion

        #region IQueryZaak
        /// <inheritdoc cref="IQueryContext.GetZaakHealthCheckAsync"/>
        async Task<HttpRequestResponse> IQueryContext.GetZaakHealthCheckAsync()
            => await this._queryZaak.GetHealthCheckAsync(this._networkService);

        /// <inheritdoc cref="IQueryContext.GetCaseAsync(Uri?)"/>
        async Task<Case> IQueryContext.GetCaseAsync(Uri? caseUri)
            => await this._queryZaak.TryGetCaseAsync(this._queryBase, caseUri);

        /// <inheritdoc cref="IQueryContext.GetCaseStatusAsync(Uri)"/>
        async Task<CaseStatus> IQueryContext.GetCaseStatusAsync(Uri caseStatusUri)
            => await this._queryZaak.TryGetCaseStatusAsync(this._queryBase, caseStatusUri);

        /// <inheritdoc cref="IQueryContext.GetCaseStatusTypeAsync(Uri)"/>
        async Task<CaseStatusType> IQueryContext.GetCaseStatusTypeAsync(Uri caseStatusTypeUri)
            => await this._queryZaak.TryGetCaseStatusTypeAsync(this._queryBase, caseStatusTypeUri);

        /// <inheritdoc cref="IQueryContext.GetCaseResultTypeAsync(Uri)"/>
        async Task<CaseResultType> IQueryContext.GetCaseResultTypeAsync(Uri? caseStatusTypeUri)
            => await this._queryZaak.TryGetCaseResultTypeAsync(this._queryBase, caseStatusTypeUri);

        /// <inheritdoc cref="IQueryContext.GetCaseStatusesAsync(Uri?)"/>
        async Task<CaseStatuses> IQueryContext.GetCaseStatusesAsync(Uri? caseUri)
            => await this._queryZaak.TryGetCaseStatusesAsync(this._queryBase, caseUri);

        /// <inheritdoc cref="IQueryContext.GetLastCaseTypeAsync(CaseStatuses?)"/>
        async Task<CaseType> IQueryContext.GetLastCaseTypeAsync(CaseStatuses? caseStatuses)
        {
            caseStatuses ??= await ((IQueryContext)this).GetCaseStatusesAsync();
            return await this._queryZaak.GetLastCaseTypeAsync(this._queryBase, caseStatuses.Value);
        }

        /// <inheritdoc cref="IQueryContext.GetBsnNumberAsync(Uri)"/>
        async Task<string> IQueryContext.GetBsnNumberAsync(Uri caseUri)
        {
            return await this._queryZaak.GetBsnNumberAsync(this._queryBase, caseUri);
        }

        /// <inheritdoc cref="IQueryContext.GetCaseTypeUriAsync(Uri?)"/>
        async Task<Uri> IQueryContext.GetCaseTypeUriAsync(Uri? caseUri)
        {
            return await this._queryZaak.TryGetCaseTypeUriAsync(this._queryBase, caseUri);
        }
        #endregion

        #region IQueryKlant
        /// <inheritdoc cref="IQueryContext.GetKlantHealthCheckAsync"/>
        async Task<HttpRequestResponse> IQueryContext.GetKlantHealthCheckAsync()
            => await this._queryKlant.GetHealthCheckAsync(this._networkService);

        /// <inheritdoc cref="IQueryContext.GetPartyDataAsync(Uri?, string?, string?)"/>
        async Task<CommonPartyData> IQueryContext.GetPartyDataAsync(Uri? caseUri, string? bsnNumber, string? caseIdentifier)
        {
            if (caseUri.IsNullOrDefault())
            {
                return bsnNumber.IsNullOrEmpty()
                    ? throw new ArgumentException(QueryResources.Querying_ERROR_Internal_MissingBsnNumber)
                    : await this._queryKlant.TryGetPartyDataAsync(this._queryBase, bsnNumber, caseIdentifier: caseIdentifier);
            }

            CaseRole caseRole = await this._queryZaak.GetCaseRoleAsync(this._queryBase, caseUri);

            if (caseRole.InvolvedPartyUri.IsNullOrDefault())
            {
                bsnNumber ??= await ((IQueryContext)this).GetBsnNumberAsync(caseUri);
                return await this._queryKlant.TryGetPartyDataAsync(this._queryBase, bsnNumber, caseIdentifier: caseIdentifier);
            }

            return await this._queryKlant.TryGetPartyDataAsync(this._queryBase, caseRole.InvolvedPartyUri, caseIdentifier);
        }

        /// <inheritdoc cref="IQueryContext.CreateNewContactMomentAsync(string)"/>
        public async Task<MaakKlantContact> CreateNewContactMomentAsync(string jsonBody)
            => await this._queryKlant.CreateNewContactMomentAsync(this._queryBase, jsonBody);

        /// <inheritdoc cref="IQueryContext.CreateContactMomentAsync(string)"/>
        async Task<ContactMoment> IQueryContext.CreateContactMomentAsync(string jsonBody)
            => await this._queryKlant.CreateContactMomentAsync(this._queryBase, jsonBody);

        /// <inheritdoc cref="IQueryContext.LinkCaseToContactMomentAsync(string)"/>
        async Task<HttpRequestResponse> IQueryContext.LinkCaseToContactMomentAsync(string jsonBody)
            => await this._queryKlant.LinkCaseToContactMomentAsync(this._networkService, jsonBody);

        /// <inheritdoc cref="IQueryContext.LinkPartyToContactMomentAsync"/>
        async Task<HttpRequestResponse> IQueryContext.LinkPartyToContactMomentAsync(string jsonBody)
            => await this._queryKlant.LinkPartyToContactMomentAsync(this._networkService, jsonBody);

        /// <inheritdoc cref="IQueryContext.LinkActorToContactMomentAsync"/>
        async Task<HttpRequestResponse> IQueryContext.LinkActorToContactMomentAsync(string jsonBody)
            => await this._queryKlant.LinkActorToContactMomentAsync(this._networkService, jsonBody);
        #endregion

        #region IQueryBesluiten
        /// <inheritdoc cref="IQueryContext.GetBesluitenHealthCheckAsync"/>
        async Task<HttpRequestResponse> IQueryContext.GetBesluitenHealthCheckAsync()
            => await this._queryBesluiten.GetHealthCheckAsync(this._networkService);

        /// <inheritdoc cref="IQueryContext.GetDecisionResourceAsync(Uri?)"/>
        async Task<DecisionResource> IQueryContext.GetDecisionResourceAsync(Uri? resourceUri)
            => await this._queryBesluiten.TryGetDecisionResourceAsync(this._queryBase, resourceUri);

        /// <inheritdoc cref="IQueryContext.GetInfoObjectAsync(object?)"/>
        async Task<InfoObject> IQueryContext.GetInfoObjectAsync(object? parameter)
            => await this._queryBesluiten.TryGetInfoObjectAsync(this._queryBase, parameter);

        /// <inheritdoc cref="IQueryContext.GetDecisionAsync(DecisionResource?)"/>
        async Task<Decision> IQueryContext.GetDecisionAsync(DecisionResource? decisionResource)
            => await this._queryBesluiten.TryGetDecisionAsync(this._queryBase, decisionResource);

        /// <inheritdoc cref="IQueryContext.GetDocumentsAsync(DecisionResource?)"/>
        async Task<Documents> IQueryContext.GetDocumentsAsync(DecisionResource? decisionResource)
            => await this._queryBesluiten.TryGetDocumentsAsync(this._queryBase, decisionResource);

        /// <inheritdoc cref="IQueryContext.GetDecisionTypeAsync(Decision?)"/>
        async Task<DecisionType> IQueryContext.GetDecisionTypeAsync(Decision? decision)
            => await this._queryBesluiten.TryGetDecisionTypeAsync(this._queryBase, decision);
        #endregion

        #region IQueryObjecten
        /// <inheritdoc cref="IQueryContext.GetObjectenHealthCheckAsync"/>
        async Task<HttpRequestResponse> IQueryContext.GetObjectenHealthCheckAsync()
            => await this._queryObjecten.GetHealthCheckAsync(this._networkService);

        /// <inheritdoc cref="IQueryContext.GetTaskAsync()"/>
        Task<CommonTaskData> IQueryContext.GetTaskAsync()
            => this._queryObjecten.GetTaskAsync(this._queryBase);

        /// <inheritdoc cref="IQueryContext.GetMessageAsync()"/>
        Task<MessageObject> IQueryContext.GetMessageAsync()
            => this._queryObjecten.GetMessageAsync(this._queryBase);

        /// <inheritdoc cref="IQueryContext.CreateObjectAsync(string)"/>
        async Task<HttpRequestResponse> IQueryContext.CreateObjectAsync(string objectJsonBody)
            => await this._queryObjecten.CreateObjectAsync(this._networkService, objectJsonBody);

        /// <inheritdoc cref="IQueryContext.DeleteObjectAsync(Guid)"/>
        async Task<HttpRequestResponse> IQueryContext.DeleteObjectAsync(Guid objectUuid)
            => await this._queryObjecten.DeleteObjectAsync(this._networkService, objectUuid);

        /// <inheritdoc cref="IQueryContext.GetKtoObjectAsync(Guid)"/>
        Task<KtoObject> IQueryContext.GetKtoObjectAsync(Guid objectUuid)
            => this._queryObjecten.GetKtoObjectAsync(this._queryBase, objectUuid);
        #endregion

        #region IQueryObjectTypen
        /// <inheritdoc cref="IQueryContext.GetObjectTypenHealthCheckAsync"/>
        async Task<HttpRequestResponse> IQueryContext.GetObjectTypenHealthCheckAsync()
            => await this._queryObjectTypen.GetHealthCheckAsync(this._networkService);

        /// <inheritdoc cref="IQueryContext.PrepareObjectJsonBody(string)"/>
        string IQueryContext.PrepareObjectJsonBody(string dataJson)
            => this._queryObjectTypen.PrepareObjectJsonBody(dataJson);
        #endregion

        #region IQueryVtb
        /// <inheritdoc cref="IQueryContext.GetMessageDataAsync(Guid)"/>
        async Task<MessageData> IQueryContext.GetMessageDataAsync(Guid messageUuid)
            => await this._queryVtb.GetMessageDataAsync(this._queryBase, messageUuid);
        #endregion

        #region Kto
        /// <inheritdoc cref="IQueryContext.SendKtoAsync"/>
        async Task<HttpRequestResponse> IQueryContext.SendKtoAsync(string body)
            => await this._networkServiceKto.PostAsync(body);
        #endregion
    }
}