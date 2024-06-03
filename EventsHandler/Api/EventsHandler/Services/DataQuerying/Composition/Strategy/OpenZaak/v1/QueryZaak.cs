// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak.v1;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.v1
{
    /// <inheritdoc cref="IQueryZaak"/>
    /// <remarks>
    ///   Version: "OpenZaak" (1.0) Web service.
    /// </remarks>
    internal sealed class QueryZaak : IQueryZaak
    {
        /// <inheritdoc cref="IQueryZaak.Configuration"/>
        WebApiConfiguration IQueryZaak.Configuration { get; set; } = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryZaak"/> class.
        /// </summary>
        public QueryZaak(WebApiConfiguration configuration)
        {
            ((IQueryZaak)this).Configuration = configuration;
        }

        #region Polymorphic (BSN Number)
        /// <inheritdoc cref="IQueryZaak.GetBsnNumberAsync(EventsHandler.Services.DataQuerying.Composition.Interfaces.IQueryBase,string)"/>
        async Task<string> IQueryZaak.GetBsnNumberAsync(IQueryBase queryBase, string openZaakDomain)
        {
            const string subjectType = "natuurlijk_persoon";  // NOTE: Only this specific parameter value is supported

            return (await GetCaseRolesV1Async(queryBase, openZaakDomain, subjectType)).Citizen.BsnNumber;
        }

        private static async Task<CaseRoles> GetCaseRolesV1Async(IQueryBase queryBase, string openZaakDomain, string subjectType)
        {
            // Predefined URL components
            string rolesEndpoint = $"https://{openZaakDomain}/zaken/api/v1/rollen";

            // Request URL
            Uri caseWithRoleUri =
                new($"{rolesEndpoint}?zaak={queryBase.Notification.MainObject}" +
                    $"&betrokkeneType={subjectType}");

            return await queryBase.ProcessGetAsync<CaseRoles>(
                httpsClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseWithRoleUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseRole);
        }
        #endregion

        #region Polimorphic (Case type)
        /// <inheritdoc cref="IQueryZaak.GetCaseTypeUriFromDetailsAsync(IQueryBase)"/>
        async Task<Uri> IQueryZaak.GetCaseTypeUriFromDetailsAsync(IQueryBase queryBase)
        {
            return (await GetCaseDetailsV1Async(queryBase)).CaseType;
        }

        private static async Task<CaseDetails> GetCaseDetailsV1Async(IQueryBase queryBase)
        {
            return await queryBase.ProcessGetAsync<CaseDetails>(
                httpsClientType: HttpClientTypes.OpenZaak_v1,
                uri: queryBase.Notification.MainObject,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseDetails);
        }
        #endregion
    }
}