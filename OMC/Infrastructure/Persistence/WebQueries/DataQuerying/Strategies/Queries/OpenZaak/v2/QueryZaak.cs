// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Versioning.Interfaces;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenZaak.Interfaces;
using WebQueries.DataSending.Clients.Enums;
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
        WebApiConfiguration IQueryZaak.Configuration { get; set; } = null!;

        /// <inheritdoc cref="IVersionDetails.Version"/>
        string IVersionDetails.Version => "1.12.1";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryZaak"/> class.
        /// </summary>
        public QueryZaak(WebApiConfiguration configuration)  // Dependency Injection (DI)
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
            string rolesEndpoint = $"https://{((IQueryZaak)this).GetDomain()}/rollen";

            // Request URL
            var caseWithRoleUri = new Uri($"{rolesEndpoint}?zaak={caseUri}");

            return await queryBase.ProcessGetAsync<CaseRoles>(  // NOTE: CaseRoles v2
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseWithRoleUri,
                fallbackErrorMessage: ZhvResources.HttpRequest_ERROR_NoCaseRole);
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