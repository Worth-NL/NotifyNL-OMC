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
        /// <inheritdoc cref="IQueryZaak.PolymorphicGetBsnNumberAsync(IQueryBase, string, Uri)"/>
        async Task<string> IQueryZaak.PolymorphicGetBsnNumberAsync(IQueryBase queryBase, string openZaakDomain, Uri caseTypeUri)
        {
            const string subjectType = "natuurlijk_persoon";  // NOTE: Only this specific parameter value is supported

            return (await GetCaseRolesV1Async(queryBase, openZaakDomain, caseTypeUri, subjectType))
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

            return await queryBase.ProcessGetAsync<CaseRoles>(  // NOTE: CaseRoles v1
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseWithRoleUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseRole);
        }
        #endregion

        #region Polymorphic (Case type URI)
        /// <inheritdoc cref="IQueryZaak.PolymorphicGetCaseTypeUriAsync(IQueryBase, Uri)"/>
        async Task<Uri> IQueryZaak.PolymorphicGetCaseTypeUriAsync(IQueryBase queryBase, Uri caseUri)
        {
            return (await GetCaseDetailsV1Async(queryBase, caseUri))
                .CaseTypeUrl;
        }

        private static async Task<CaseDetails> GetCaseDetailsV1Async(IQueryBase queryBase, Uri caseUri)
        {
            return await queryBase.ProcessGetAsync<CaseDetails>(  // NOTE: CaseDetails v1
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseUri,  // Request URL
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseDetails);
        }
        #endregion

        #region Polymorphic (Telemetry)
        /// <summary>
        /// Sends the completion feedback to "OpenZaak" Web API service.
        /// </summary>
        /// <param name="networkService"><inheritdoc cref="IHttpNetworkService" path="/summary"/></param>
        /// <param name="openZaakDomain">The domain of <see cref="IQueryZaak"/> Web API service.</param>
        /// <param name="jsonBody">The content in JSON format to be passed with POST request as HTTP Request Body.</param>
        /// <returns>
        ///   The JSON response from an external Telemetry Web API service.
        /// </returns>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="TelemetryException"/>
        internal static async Task<string> SendFeedbackAsync(IHttpNetworkService networkService, string openZaakDomain, string jsonBody)
        {
            // Predefined URL components
            var klantContactMomentUri = new Uri($"https://{openZaakDomain}/zaken/api/v1/zaakcontactmomenten");

            // Sending the request
            (bool success, string jsonResponse) = await networkService.PostAsync(
                httpClientType: HttpClientTypes.Telemetry_Contactmomenten,
                uri: klantContactMomentUri,  // Request URL
                jsonBody);

            // Getting the response
            return success ? jsonResponse : throw new TelemetryException(jsonResponse);
        }
        #endregion
    }
}