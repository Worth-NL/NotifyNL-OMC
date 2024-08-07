// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.Objecten;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.Objecten.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.ObjectTypen.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.v2;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;

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
    /// <seealso cref="IQueryObjectTypen"/>
    /// <seealso cref="IQueryObjecten"/>
    internal interface IQueryContext
    {
        #region IQueryBase
        /// <inheritdoc cref="IQueryBase.Notification"/>
        internal void SetNotification(NotificationEvent notificationEvent);
        #endregion

        #region IQueryZaak
        /// <inheritdoc cref="IQueryZaak.GetCaseAsync(IQueryBase, object?)"/>
        /// <remarks>
        ///   The <see cref="Case"/> can be queried either directly from the provided <see cref="Uri"/>, or domain object, or it can
        ///   be extracted internally from the queried case details (cost is an additional overhead) from "OpenZaak" Web API service.
        /// </remarks>
        internal Task<Case> GetCaseAsync(object? parameter = null);

        /// <inheritdoc cref="IQueryZaak.GetCaseStatusesAsync(IQueryBase)"/>
        internal Task<CaseStatuses> GetCaseStatusesAsync();

        /// <inheritdoc cref="IQueryZaak.GetLastCaseTypeAsync(IQueryBase, CaseStatuses)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing <see cref="CaseStatuses"/>, but it produces an additional
        ///   overhead since the missing statuses will be queried internally anyway from "OpenZaak" Web API service.
        /// </remarks>
        internal Task<CaseType> GetLastCaseTypeAsync(CaseStatuses? statuses = null);

        /// <inheritdoc cref="IQueryZaak.GetBsnNumberAsync(IQueryBase)"/>
        internal Task<string> GetBsnNumberAsync();

        /// <inheritdoc cref="IQueryZaak.GetBsnNumberAsync(IQueryBase, Uri)"/>
        internal Task<string> GetBsnNumberAsync(Uri caseTypeUri);

        /// <inheritdoc cref="IQueryZaak.GetMainObjectAsync(IQueryBase)"/>
        internal Task<MainObject> GetMainObjectAsync();

        /// <inheritdoc cref="IQueryZaak.GetInfoObjectAsync(IQueryBase)"/>
        internal Task<InfoObject> GetInfoObjectAsync();

        /// <inheritdoc cref="IQueryZaak.SendFeedbackAsync(IHttpNetworkService, HttpContent)"/>
        internal Task<string> SendFeedbackToOpenZaakAsync(HttpContent body);
        #endregion

        #region IQueryKlant
        /// <inheritdoc cref="IQueryKlant.GetPartyDataAsync(IQueryBase, string)"/>
        /// <remarks>
        ///   Simpler usage doesn't require providing BSN number first, but it produces an additional
        ///   overhead since the missing BSN will be queried internally anyway from "OpenZaak" Web API service.
        /// </remarks>
        internal Task<CommonPartyData> GetPartyDataAsync(string? bsnNumber = null);

        /// <inheritdoc cref="IQueryKlant.SendFeedbackAsync(IQueryBase, HttpContent)"/>
        internal Task<ContactMoment> SendFeedbackToOpenKlantAsync(HttpContent body);

        // NOTE: This method is different between IQueryZaak from "OMC workflow v1" and "OMC workflow v2",
        //       because it's not sending any requests to "OpenZaak" Web API service anymore. Due to that,
        //       the IQueryZaak interface cannot be used directly (from logical or business point of view)
        /// <inheritdoc cref="QueryKlant.LinkToSubjectObjectAsync(IHttpNetworkService, string, HttpContent)"/>
        internal Task<string> LinkToSubjectObjectAsync(HttpContent body);
        #endregion

        #region IQueryObjecten
        /// <inheritdoc cref="IQueryObjecten.GetTaskAsync(IQueryBase)"/>
        internal Task<TaskObject> GetTaskAsync();
        #endregion
    }
}