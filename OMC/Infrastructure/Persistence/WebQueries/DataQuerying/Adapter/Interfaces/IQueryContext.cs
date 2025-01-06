// © 2024, Worth Systems.

using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries;
using WebQueries.DataQuerying.Strategies.Queries.Besluiten.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.Objecten.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.ObjectTypen.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenKlant.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenZaak.Interfaces;
using WebQueries.DataSending.Interfaces;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;
using ZhvModels.Mapping.Models.POCOs.Objecten.Message;
using ZhvModels.Mapping.Models.POCOs.Objecten.Task;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;
using ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision;

namespace WebQueries.DataQuerying.Adapter.Interfaces
{
    /// <summary>
    /// The adapter combining and adjusting functionalities from other data querying services.
    /// </summary>
    /// <remarks>
    ///   This interface is modifying signatures of methods from related query services to hide some dependencies
    ///   inside the <see cref="IQueryContext"/> implementation, make the usage of these methods easier, and base
    ///   on the injected/setup context.
    /// </remarks>
    /// <seealso cref="IQueryBase"/>
    /// <seealso cref="IQueryKlant"/>
    /// <seealso cref="IQueryZaak"/>
    /// <seealso cref="IQueryBesluiten"/>
    /// <seealso cref="IQueryObjecten"/>
    /// <seealso cref="IQueryObjectTypen"/>
    public interface IQueryContext
    {
        #region IQueryBase
        /// <inheritdoc cref="IQueryBase.Notification"/>
        internal void SetNotification(NotificationEvent notificationEvent);
        #endregion

        #region IQueryZaak
        /// <inheritdoc cref="IDomain.GetHealthCheckAsync(IHttpNetworkService)"/>
        public Task<HttpRequestResponse> GetZaakHealthCheckAsync();

        /// <inheritdoc cref="IQueryZaak.TryGetCaseAsync(IQueryBase, Uri?)"/>
        /// <remarks>
        ///   The <see cref="Case"/> can be queried either directly from the provided <see cref="Uri"/>, or domain object, or it can
        ///   be extracted internally from the queried case details (cost is an additional overhead) from "OpenZaak" Web API service.
        /// </remarks>
        public Task<Case> GetCaseAsync(Uri? caseUri = null);

        /// <inheritdoc cref="IQueryZaak.TryGetCaseStatusesAsync(IQueryBase, Uri?)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing <see cref="Case"/> <see cref="Uri"/>.
        ///   <para>
        ///     NOTE: However, in this case the missing <seealso cref="Uri"/> will be attempted to retrieve
        ///     directly from the initial notification <see cref="NotificationEvent.MainObjectUri"/> (which
        ///     will work only if the notification was meant to be used with Case scenarios).
        ///   </para>
        /// </remarks>
        public Task<CaseStatuses> GetCaseStatusesAsync(Uri? caseUri = null);

        /// <inheritdoc cref="IQueryZaak.GetLastCaseTypeAsync(IQueryBase, CaseStatuses)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing <see cref="CaseStatuses"/>, but it produces an additional
        ///   overhead since the missing statuses will be queried internally anyway from "OpenZaak" Web API service.
        /// </remarks>
        public Task<CaseType> GetLastCaseTypeAsync(CaseStatuses? caseStatuses = null);

        /// <inheritdoc cref="IQueryZaak.GetBsnNumberAsync(IQueryBase, Uri)"/>
        public Task<string> GetBsnNumberAsync(Uri caseUri);

        /// <inheritdoc cref="IQueryZaak.TryGetCaseTypeUriAsync(IQueryBase, Uri?)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing <see cref="Case"/> <see cref="Uri"/> as query parameter.
        ///   <para>
        ///     NOTE: Bear in mind that if the <see cref="Case"/> <see cref="Uri"/> is missing, the looked
        ///     up <see cref="CaseType"/> <see cref="Uri"/> will be attempted to retrieve directly from the
        ///     initial notification from <see cref="EventAttributes.CaseTypeUri"/> (which will work only if
        ///     the notification was meant to be used with Case scenarios).
        ///   </para>
        /// </remarks>
        public Task<Uri> GetCaseTypeUriAsync(Uri? caseUri = null);
        #endregion

        #region IQueryKlant
        /// <inheritdoc cref="IDomain.GetHealthCheckAsync(IHttpNetworkService)"/>
        public Task<HttpRequestResponse> GetKlantHealthCheckAsync();

        /// <summary>
        /// <inheritdoc cref="IQueryKlant.TryGetPartyDataAsync(IQueryBase, string)"/>
        /// </summary>
        /// <remarks>
        ///   Simpler usage doesn't require providing BSN number first, but it produces an additional
        ///   overhead since the missing BSN will be queried internally anyway from "OpenZaak" Web API service.
        ///   <para>
        ///     NOTE: While querying party details (e.g., citizen or organization) the <see cref="Case"/> <see cref="Uri"/> can be
        ///     used to get <see cref="CaseRole"/> and based on that determine whether the organization data will be retrieved, or
        ///     rather the citizen data.
        ///     <para>
        ///       In case of getting citizen data the BSN number should be provided; otherwise, the missing BSN will be obtained using
        ///       the provided <see cref="Case"/> <see cref="Uri"/> which, if missing, will be attempted to be obtained from the initial
        ///       notification <see cref="NotificationEvent.MainObjectUri"/> (which will work only if the notification was meant to be
        ///       used with Case scenarios).
        ///     </para>
        ///   </para>
        /// </remarks>
        public Task<CommonPartyData> GetPartyDataAsync(Uri? caseUri, string? bsnNumber = null);

        /// <inheritdoc cref="IQueryKlant.CreateContactMomentAsync(IQueryBase, string)"/>
        public Task<ContactMoment> CreateContactMomentAsync(string jsonBody);

        /// <inheritdoc cref="IQueryKlant.LinkCaseToContactMomentAsync(IHttpNetworkService, string)"/>
        public Task<HttpRequestResponse> LinkCaseToContactMomentAsync(string jsonBody);

        /// <inheritdoc cref="IQueryKlant.LinkPartyToContactMomentAsync"/>
        public Task<HttpRequestResponse> LinkPartyToContactMomentAsync(string jsonBody);
        #endregion

        #region IQueryBesluiten
        /// <inheritdoc cref="IDomain.GetHealthCheckAsync(IHttpNetworkService)"/>
        public Task<HttpRequestResponse> GetBesluitenHealthCheckAsync();

        /// <inheritdoc cref="IQueryBesluiten.TryGetDecisionResourceAsync(IQueryBase, Uri?)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing resource <see cref="Uri"/>.
        ///   <para>
        ///     NOTE: However, in this case the missing <seealso cref="Uri"/> will be attempted to retrieve
        ///     directly from the initial notification <see cref="NotificationEvent.ResourceUri"/> (which will
        ///     work only if the notification was meant to be used with Decision scenarios).
        ///   </para>
        /// </remarks>
        public Task<DecisionResource> GetDecisionResourceAsync(Uri? resourceUri = null);

        /// <inheritdoc cref="IQueryBesluiten.TryGetInfoObjectAsync(IQueryBase, object?)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing <see cref="DecisionResource"/>, but it produces an additional
        ///   overhead since the missing resource will be queried internally anyway from "OpenZaak" Web API service.
        /// </remarks>
        public Task<InfoObject> GetInfoObjectAsync(object? parameter = null);

        /// <inheritdoc cref="IQueryBesluiten.TryGetDecisionAsync(IQueryBase, DecisionResource?)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing <see cref="DecisionResource"/>, but it produces an additional
        ///   overhead since the missing resource will be queried internally anyway from "OpenZaak" Web API service.
        /// </remarks>
        public Task<Decision> GetDecisionAsync(DecisionResource? decisionResource = null);

        /// <inheritdoc cref="IQueryBesluiten.TryGetDocumentsAsync(IQueryBase, DecisionResource?)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing <see cref="DecisionResource"/>, but it produces an additional
        ///   overhead since the missing resource will be queried internally anyway from "OpenZaak" Web API service.
        /// </remarks>
        public Task<Documents> GetDocumentsAsync(DecisionResource? decisionResource = null);

        /// <inheritdoc cref="IQueryBesluiten.TryGetDecisionTypeAsync(IQueryBase, Decision?)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing <see cref="Decision"/>, but it produces an additional
        ///   overhead since the missing object will be re-queried internally anyway from "OpenZaak" Web API
        ///   service. One of alternatives is that just before querying <see cref="Decision"/> to get desired
        ///   <seealso cref="DecisionType"/> <see cref="Uri"/> will be attempted to retrieve directly from
        ///   the initial notification from <see cref="EventAttributes.DecisionTypeUri"/> (which will work
        ///   only if the notification was meant to be used with Decision scenarios).
        /// </remarks>
        public Task<DecisionType> GetDecisionTypeAsync(Decision? decision = null);
        #endregion

        #region IQueryObjecten
        /// <inheritdoc cref="IDomain.GetHealthCheckAsync(IHttpNetworkService)"/>
        public Task<HttpRequestResponse> GetObjectenHealthCheckAsync();

        /// <inheritdoc cref="IQueryObjecten.GetTaskAsync(IQueryBase)"/>
        public Task<CommonTaskData> GetTaskAsync();

        /// <inheritdoc cref="IQueryObjecten.GetMessageAsync(IQueryBase)"/>
        public Task<MessageObject> GetMessageAsync();

        /// <inheritdoc cref="IQueryObjecten.CreateObjectAsync(IHttpNetworkService, string)"/>
        public Task<HttpRequestResponse> CreateObjectAsync(string objectJsonBody);
        #endregion

        #region IQueryObjectTypen
        /// <inheritdoc cref="IDomain.GetHealthCheckAsync(IHttpNetworkService)"/>
        public Task<HttpRequestResponse> GetObjectTypenHealthCheckAsync();

        /// <inheritdoc cref="IQueryObjectTypen.PrepareObjectJsonBody(string)"/>
        public string PrepareObjectJsonBody(string dataJson);
        #endregion
    }
}