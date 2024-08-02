// © 2024, Worth Systems.

using EventsHandler.Exceptions;
using EventsHandler.Mapping.Models.POCOs.OpenZaak.v1;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces;
using EventsHandler.Services.DataSending.Clients.Enums;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Versioning.Interfaces;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.v1
{
    /// <inheritdoc cref="IQueryZaak"/>
    /// <remarks>
    ///   Version: "OpenZaak" (v1+) Web API service | "OMC workflow" v1.
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
            const string subjectType = "natuurlijk_persoon";  // NOTE: Only this specific parameter value is supported

            return (await GetCaseRolesV1Async(queryBase, ((IQueryZaak)this).GetDomain(), caseTypeUri, subjectType))
                .Citizen
                .BsnNumber;
        }

        private static async Task<CaseRoles> GetCaseRolesV1Async(IQueryBase queryBase, string openZaakDomain, Uri caseTypeUri, string subjectType)
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
        /// <inheritdoc cref="IQueryZaak.RequestCaseTypeUriAsync"/>
        async Task<Uri> IQueryZaak.RequestCaseTypeUriAsync(IQueryBase queryBase, Uri caseUri)
        {
            return (await GetCaseDetailsV1Async(queryBase, caseUri))
                .CaseTypeUrl;
        }

        private static async Task<CaseDetails> GetCaseDetailsV1Async(IQueryBase queryBase, Uri caseUri)
        {
            return await queryBase.ProcessGetAsync<CaseDetails>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseDetails);
        }
        #endregion

        #region Polymorphic (Telemetry)
        /// <inheritdoc cref="IQueryZaak.SendFeedbackAsync(IHttpNetworkService, HttpContent)"/>
        async Task<string> IQueryZaak.SendFeedbackAsync(IHttpNetworkService networkService, HttpContent body)
        {
            // Predefined URL components
            var klantContactMomentUri = new Uri($"https://{((IQueryZaak)this).GetDomain()}/zaken/api/v1/zaakcontactmomenten");

            // Sending the request
            (bool success, string jsonResponse) = await networkService.PostAsync(
                httpClientType: HttpClientTypes.Telemetry_Contactmomenten,
                uri: klantContactMomentUri,
                body: body);

            // Getting the response
            return success ? jsonResponse : throw new TelemetryException(jsonResponse);
        }
        #endregion
    }
}