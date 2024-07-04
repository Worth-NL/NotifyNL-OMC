// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak.v2;
using EventsHandler.Behaviors.Versioning;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;
using EventsHandler.Services.DataReceiving.Interfaces;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.v2
{
    /// <inheritdoc cref="IQueryZaak"/>
    /// <remarks>
    ///   Version: "OpenZaak" (v1+) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    internal sealed class QueryZaak : IQueryZaak
    {
        /// <inheritdoc cref="IQueryZaak.Configuration"/>
        WebApiConfiguration IQueryZaak.Configuration { get; set; } = null!;
        
        /// <inheritdoc cref="IVersionDetails.Version"/>
        string IVersionDetails.Version => "1.12.1";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryZaak"/> class.
        /// </summary>
        public QueryZaak(WebApiConfiguration configuration)
        {
            ((IQueryZaak)this).Configuration = configuration;
        }

        #region Polymorphic (BSN Number)
        /// <inheritdoc cref="IQueryZaak.GetBsnNumberAsync(IQueryBase)"/>
        async Task<string> IQueryZaak.GetBsnNumberAsync(IQueryBase queryBase)
        {
            return await ((IQueryZaak)this).GetBsnNumberAsync(queryBase, queryBase.Notification.MainObject);
        }

        /// <inheritdoc cref="IQueryZaak.GetBsnNumberAsync(IQueryBase, Uri)"/>
        async Task<string> IQueryZaak.GetBsnNumberAsync(IQueryBase queryBase, Uri caseTypeUri)
        {
            string subjectType = ((IQueryZaak)this).Configuration.AppSettings.Variables.SubjectType();  // NOTE: Multiple parameter values can be supported

            return (await GetCaseRolesV2Async(queryBase, ((IQueryZaak)this).GetDomain(), caseTypeUri, subjectType))
                .Citizen(((IQueryZaak)this).Configuration)
                .BsnNumber;
        }

        private static async Task<CaseRoles> GetCaseRolesV2Async(IQueryBase queryBase, string openZaakDomain, Uri caseTypeUri, string subjectType)
        {
            // Predefined URL components
            string rolesEndpoint = $"https://{openZaakDomain}/zaken/api/v1/rollen";

            // Request URL
            var caseWithRoleUri = new Uri($"{rolesEndpoint}?zaak={caseTypeUri}" +
                                          $"&betrokkeneType={subjectType}");

            return await queryBase.ProcessGetAsync<CaseRoles>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseWithRoleUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseRole);
        }
        #endregion

        #region Polimorphic (Case type)
        /// <inheritdoc cref="IQueryZaak.GetCaseTypeUriFromDetailsAsync(IQueryBase)"/>
        async Task<Uri> IQueryZaak.GetCaseTypeUriFromDetailsAsync(IQueryBase queryBase)
        {
            return (await GetCaseDetailsV2Async(queryBase))
                .CaseTypeUrl;
        }
        
        private static async Task<CaseDetails> GetCaseDetailsV2Async(IQueryBase queryBase)
        {
            return await queryBase.ProcessGetAsync<CaseDetails>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: queryBase.Notification.MainObject,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseDetails);
        }
        #endregion

        #region Polymorphic (Telemetry)
        /// <inheritdoc cref="IQueryZaak.SendFeedbackAsync(IHttpNetworkService, HttpContent)"/>
        Task<string> IQueryZaak.SendFeedbackAsync(IHttpNetworkService networkService, HttpContent body)
        {
            throw new NotImplementedException(Resources.HttpRequest_ERROR_TelemetryOpenZaakNotImplemented);  // TODO: To be removed and converted to static in OpenZaak v1
        }
        #endregion
    }
}