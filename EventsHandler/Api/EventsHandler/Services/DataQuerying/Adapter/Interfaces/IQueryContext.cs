// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.Objecten.Message;
using EventsHandler.Mapping.Models.POCOs.Objecten.Task;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Mapping.Models.POCOs.OpenZaak.Decision;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.Besluiten.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.Objecten.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.ObjectTypen.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.DataSending.Responses;

namespace EventsHandler.Services.DataQuerying.Adapter.Interfaces
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
    internal interface IQueryContext
    {
        #region IQueryBase
        /// <inheritdoc cref="IQueryBase.Notification"/>
        internal void SetNotification(NotificationEvent notificationEvent);
        #endregion

        #region IQueryZaak
        /// <inheritdoc cref="IQueryZaak.TryGetCaseAsync(IQueryBase, Uri?)"/>
        /// <remarks>
        ///   The <see cref="Case"/> can be queried either directly from the provided <see cref="Uri"/>, or domain object, or it can
        ///   be extracted internally from the queried case details (cost is an additional overhead) from "OpenZaak" Web API service.
        /// </remarks>
        internal Task<Case> GetCaseAsync(Uri? caseUri = null);

        /// <inheritdoc cref="IQueryZaak.TryGetCaseStatusesAsync(IQueryBase, Uri?)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing <see cref="Case"/> <see cref="Uri"/>.
        ///   <para>
        ///     NOTE: However, in this case the missing <seealso cref="Uri"/> will be attempted to retrieve
        ///     directly from the initial notification <see cref="NotificationEvent.MainObjectUri"/> (which
        ///     will work only if the notification was meant to be used with Case scenarios).
        ///   </para>
        /// </remarks>
        internal Task<CaseStatuses> GetCaseStatusesAsync(Uri? caseUri = null);

        /// <inheritdoc cref="IQueryZaak.GetLastCaseTypeAsync(IQueryBase, CaseStatuses)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing <see cref="CaseStatuses"/>, but it produces an additional
        ///   overhead since the missing statuses will be queried internally anyway from "OpenZaak" Web API service.
        /// </remarks>
        internal Task<CaseType> GetLastCaseTypeAsync(CaseStatuses? caseStatuses = null);

        /// <inheritdoc cref="IQueryZaak.GetMainObjectAsync(IQueryBase)"/>
        internal Task<MainObject> GetMainObjectAsync();

        /// <inheritdoc cref="IQueryZaak.TryGetDecisionResourceAsync(IQueryBase, Uri?)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing resource <see cref="Uri"/>.
        ///   <para>
        ///     NOTE: However, in this case the missing <seealso cref="Uri"/> will be attempted to retrieve
        ///     directly from the initial notification <see cref="NotificationEvent.ResourceUri"/> (which will
        ///     work only if the notification was meant to be used with Decision scenarios).
        ///   </para>
        /// </remarks>
        internal Task<DecisionResource> GetDecisionResourceAsync(Uri? resourceUri = null);

        /// <inheritdoc cref="IQueryZaak.TryGetInfoObjectAsync(IQueryBase, object?)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing <see cref="DecisionResource"/>, but it produces an additional
        ///   overhead since the missing resource will be queried internally anyway from "OpenZaak" Web API service.
        /// </remarks>
        internal Task<InfoObject> GetInfoObjectAsync(object? parameter = null);

        /// <inheritdoc cref="IQueryZaak.TryGetDecisionAsync(IQueryBase, DecisionResource?)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing <see cref="DecisionResource"/>, but it produces an additional
        ///   overhead since the missing resource will be queried internally anyway from "OpenZaak" Web API service.
        /// </remarks>
        internal Task<Decision> GetDecisionAsync(DecisionResource? decisionResource = null);

        /// <inheritdoc cref="IQueryZaak.TryGetDocumentsAsync(IQueryBase, DecisionResource?)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing <see cref="DecisionResource"/>, but it produces an additional
        ///   overhead since the missing resource will be queried internally anyway from "OpenZaak" Web API service.
        /// </remarks>
        internal Task<Documents> GetDocumentsAsync(DecisionResource? decisionResource = null);

        /// <inheritdoc cref="IQueryZaak.TryGetDecisionTypeAsync(IQueryBase, Decision?)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing <see cref="Decision"/>, but it produces an additional
        ///   overhead since the missing object will be re-queried internally anyway from "OpenZaak" Web API
        ///   service. One of alternatives is that just before querying <see cref="Decision"/> to get desired
        ///   <seealso cref="DecisionType"/> <see cref="Uri"/> will be attempted to retrieve directly from
        ///   the initial notification from <see cref="EventAttributes.DecisionTypeUri"/> (which will work
        ///   only if the notification was meant to be used with Decision scenarios).
        /// </remarks>
        internal Task<DecisionType> GetDecisionTypeAsync(Decision? decision = null);

        /// <inheritdoc cref="IQueryZaak.GetBsnNumberAsync(IQueryBase, string, Uri)"/>
        internal Task<string> GetBsnNumberAsync(Uri caseUri);

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
        internal Task<Uri> GetCaseTypeUriAsync(Uri? caseUri = null);
        #endregion

        #region IQueryKlant
        /// <inheritdoc cref="IQueryKlant.TryGetPartyDataAsync(IQueryBase, string, string)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing BSN number first, but it produces an additional
        ///   overhead since the missing BSN will be queried internally anyway from "OpenZaak" Web API service.
        ///   <para>
        ///     NOTE: While querying BSN the missing <see cref="CaseType"/> <see cref="Uri"/> will be attempted to
        ///     retrieve directly from the initial notification from <seealso cref="NotificationEvent.MainObjectUri"/>
        ///     (which is <see cref="CaseType"/> <see cref="Uri"/> for Case scenarios).
        ///   </para>
        /// </remarks>
        internal Task<CommonPartyData> GetPartyDataAsync(string? bsnNumber = null);

        /// <inheritdoc cref="IQueryKlant.CreateContactMomentAsync(IQueryBase, string, string)"/>
        internal Task<ContactMoment> CreateContactMomentAsync(string jsonBody);

        /// <inheritdoc cref="IQueryKlant.LinkCaseToContactMomentAsync(IHttpNetworkService, string, string)"/>
        internal Task<RequestResponse> LinkCaseToContactMomentAsync(string jsonBody);

        /// <inheritdoc cref="IQueryKlant.LinkCustomerToContactMomentAsync(IHttpNetworkService, string, string)"/>
        internal Task<RequestResponse> LinkCustomerToContactMomentAsync(string jsonBody);
        #endregion

        #region IQueryBesluiten
        #endregion

        #region IQueryObjecten
        /// <inheritdoc cref="IQueryObjecten.GetTaskAsync(IQueryBase)"/>
        internal Task<CommonTaskData> GetTaskAsync();

        /// <inheritdoc cref="IQueryObjecten.GetMessageAsync(IQueryBase)"/>
        internal Task<MessageObject> GetMessageAsync();

        /// <inheritdoc cref="IQueryObjecten.CreateObjectAsync(IHttpNetworkService, string)"/>
        internal Task<RequestResponse> CreateObjectAsync(string objectJsonBody);
        #endregion

        #region IQueryObjectTypen
        /// <inheritdoc cref="IQueryObjectTypen.PrepareObjectJsonBody(string)"/>
        internal string PrepareObjectJsonBody(string dataJson);
        #endregion
    }
}