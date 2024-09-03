﻿// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.OpenZaak.v2;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces;
using EventsHandler.Services.DataSending.Clients.Enums;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Versioning.Interfaces;
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
        /// <inheritdoc cref="IQueryZaak.PolymorphicGetBsnNumberAsync(IQueryBase, string, Uri)"/>
        async Task<string> IQueryZaak.PolymorphicGetBsnNumberAsync(IQueryBase queryBase, string openZaakDomain, Uri caseUri)
        {
            string subjectType = ((IQueryZaak)this).Configuration.AppSettings.Variables.SubjectType();  // NOTE: Multiple parameter values can be supported

            return (await GetCaseRolesV2Async(queryBase, openZaakDomain, caseUri, subjectType))
                .Citizen(((IQueryZaak)this).Configuration)
                .BsnNumber;
        }

        private static async Task<CaseRoles> GetCaseRolesV2Async(IQueryBase queryBase, string openZaakDomain, Uri caseUri, string subjectType)
        {
            // Predefined URL components
            string rolesEndpoint = $"https://{openZaakDomain}/zaken/api/v1/rollen";

            // Request URL
            var caseWithRoleUri = new Uri($"{rolesEndpoint}?zaak={caseUri}" +
                                          $"&betrokkeneType={subjectType}");

            return await queryBase.ProcessGetAsync<CaseRoles>(  // NOTE: CaseRoles v2
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseWithRoleUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseRole);
        }
        #endregion

        #region Polymorphic (Case type URI)
        /// <inheritdoc cref="IQueryZaak.PolymorphicGetCaseTypeUriAsync(IQueryBase, Uri)"/>
        async Task<Uri> IQueryZaak.PolymorphicGetCaseTypeUriAsync(IQueryBase queryBase, Uri caseUri)
        {
            return (await GetCaseDetailsV2Async(queryBase, caseUri))
                .CaseTypeUrl;
        }

        private static async Task<CaseDetails> GetCaseDetailsV2Async(IQueryBase queryBase, Uri caseUri)
        {
            return await queryBase.ProcessGetAsync<CaseDetails>(  // NOTE: CaseDetails v2
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseUri,  // Request URL
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseDetails);
        }
        #endregion
    }
}