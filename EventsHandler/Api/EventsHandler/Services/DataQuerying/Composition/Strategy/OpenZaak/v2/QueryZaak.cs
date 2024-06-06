// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak.v2;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.v2
{
    /// <inheritdoc cref="IQueryZaak"/>
    /// <remarks>
    ///   Version: "OpenZaak" (1.0) Web API service | "OMC workflow" v2.
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
            string subjectType = ((IQueryZaak)this).Configuration.AppSettings.Variables.SubjectType();  // NOTE: Multiple parameter values can be supported

            return (await GetCaseRolesV2Async(queryBase, openZaakDomain, subjectType))
                .Citizen(((IQueryZaak)this).Configuration)
                .BsnNumber;
        }

        private static async Task<CaseRoles> GetCaseRolesV2Async(IQueryBase queryBase, string openZaakDomain, string subjectType)
        {
            // Predefined URL components
            string rolesEndpoint = $"https://{openZaakDomain}/zaken/api/v1/rollen";

            // Request URL
            Uri caseWithRoleUri =
                new($"{rolesEndpoint}?zaak={queryBase.Notification.MainObject}" +
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
            return (await GetCaseDetailsV2Async(queryBase)).CaseType;
        }
        
        private static async Task<CaseDetails> GetCaseDetailsV2Async(IQueryBase queryBase)
        {
            return await queryBase.ProcessGetAsync<CaseDetails>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: queryBase.Notification.MainObject,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseDetails);
        }
        #endregion
    }
}