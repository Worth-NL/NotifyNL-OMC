// © 2024, Worth Systems.

using Common.Settings.Configuration;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenZaak.Interfaces;
using WebQueries.DataSending.Clients.Enums;
using WebQueries.Versioning.Interfaces;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;
using ZhvModels.Mapping.Models.POCOs.OpenZaak.v2;
using ZhvModels.Properties;

namespace WebQueries.DataQuerying.Strategies.Queries.OpenZaak.v2
{
    /// <inheritdoc cref="IQueryZaak"/>
    /// <remarks>
    ///   Version: "OpenZaak" (v2) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    public sealed class QueryZaak : IQueryZaak
    {
        /// <inheritdoc cref="IQueryZaak.Configuration"/>
        OmcConfiguration IQueryZaak.Configuration { get; set; } = null!;

        /// <inheritdoc cref="IVersionDetails.Version"/>
        string IVersionDetails.Version => "1.2.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryZaak"/> class.
        /// </summary>
        public QueryZaak(OmcConfiguration configuration)  // Dependency Injection (DI)
        {
            ((IQueryZaak)this).Configuration = configuration;
        }

        #region Polymorphic (Case Role)
        /// <inheritdoc cref="IQueryZaak.GetCaseRoleAsync(IQueryBase, Uri)"/>
        async Task<CaseRole> IQueryZaak.GetCaseRoleAsync(IQueryBase queryBase, Uri caseUri)
        {
            string subjectType = ((IQueryZaak)this).Configuration.AppSettings.Variables.SubjectType();  // NOTE: Multiple parameter values can be supported

            return (await GetCaseRolesV2Async(queryBase, caseUri, subjectType))
                .CaseRole(((IQueryZaak)this).Configuration);
        }

        private async Task<CaseRoles> GetCaseRolesV2Async(IQueryBase queryBase, Uri caseUri, string _)  // TODO: Not used yet (at the moment, consulting)
        {
            // Predefined URL components
            string rolesEndpoint = $"{((IQueryZaak)this).GetDomain()}/rollen";

            // Request URL
            var caseWithRoleUri = new Uri($"{rolesEndpoint}?zaak={caseUri}");

            return await queryBase.ProcessGetAsync<CaseRoles>(  // NOTE: CaseRoles v2
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseWithRoleUri,
                fallbackErrorMessage: ZhvResources.HttpRequest_ERROR_NoCaseRole);
        }

        /// <summary>
        /// Check if case initiator is a natural person (natuurlijk persoon) based on the case URI.
        /// </summary>
        /// <param name="queryBase"></param>
        /// <param name="caseUri"></param>
        /// <returns></returns>
        public async Task<bool> CheckIfInitiatorIsNaturalPersonAsync(IQueryBase queryBase, Uri caseUri)
        {
            string rolesEndpoint = $"{((IQueryZaak)this).GetDomain()}/rollen";

            var queryParams = new List<string>
            {
                $"zaak={Uri.EscapeDataString(caseUri.ToString())}",
                "omschrijvingGeneriek=initiator",
                "betrokkeneType=natuurlijk_persoon",
                "pageSize=1"
            };
            string query = string.Join("&", queryParams);
            var requestUri = new Uri($"{rolesEndpoint}?{query}");

            CaseRoles response = await queryBase.ProcessGetAsync<CaseRoles>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: requestUri,
                fallbackErrorMessage: ZhvResources.HttpRequest_ERROR_NoCaseRole);

            // Since Results is never null (initialized as empty list), we can simply:
            return response.Results.Any();
        }
        #endregion

        #region Polymorphic (Case type URI)
        /// <inheritdoc cref="IQueryZaak.GetCaseTypeUriAsync(IQueryBase, Uri)"/>
        async Task<Uri> IQueryZaak.GetCaseTypeUriAsync(IQueryBase queryBase, Uri caseUri)
        {
            return (await GetCaseDetailsV2Async(queryBase, caseUri))
                .CaseTypeUrl;
        }

        private static async Task<CaseDetails> GetCaseDetailsV2Async(IQueryBase queryBase, Uri caseUri)
        {
            return await queryBase.ProcessGetAsync<CaseDetails>(  // NOTE: CaseDetails v2
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseUri,  // Request URL
                fallbackErrorMessage: ZhvResources.HttpRequest_ERROR_NoCaseDetails);
        }
        #endregion
    }
}