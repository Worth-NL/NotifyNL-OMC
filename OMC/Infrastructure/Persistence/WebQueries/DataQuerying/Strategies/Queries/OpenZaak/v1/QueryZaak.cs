// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Versioning.Interfaces;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenZaak.Interfaces;
using WebQueries.DataSending.Clients.Enums;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;
using ZhvModels.Mapping.Models.POCOs.OpenZaak.v1;
using ZhvModels.Properties;

namespace WebQueries.DataQuerying.Strategies.Queries.OpenZaak.v1
{
    /// <inheritdoc cref="IQueryZaak"/>
    /// <remarks>
    ///   Version: "OpenZaak" (v1) Web API service | "OMC workflow" v1.
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
            const string subjectType = "natuurlijk_persoon";  // NOTE: Only this specific parameter value is supported

            return (await GetCaseRolesV1Async(queryBase, caseUri, subjectType))
                .CaseRole(((IQueryZaak)this).Configuration);
        }

        private async Task<CaseRoles> GetCaseRolesV1Async(IQueryBase queryBase, Uri caseUri, string subjectType)
        {
            // Predefined URL components
            string rolesEndpoint = $"https://{((IQueryZaak)this).GetDomain()}/rollen";

            // Request URL
            var caseWithRoleUri = new Uri($"{rolesEndpoint}?zaak={caseUri}" +
                                          $"&betrokkeneType={subjectType}");

            return await queryBase.ProcessGetAsync<CaseRoles>(  // NOTE: CaseRoles v1
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseWithRoleUri,
                fallbackErrorMessage: ZhvResources.HttpRequest_ERROR_NoCaseRole);
        }
        #endregion

        #region Polymorphic (Case type URI)
        /// <inheritdoc cref="IQueryZaak.GetCaseTypeUriAsync(IQueryBase, Uri)"/>
        async Task<Uri> IQueryZaak.GetCaseTypeUriAsync(IQueryBase queryBase, Uri caseUri)
        {
            return (await GetCaseDetailsV1Async(queryBase, caseUri))
                .CaseTypeUrl;
        }

        private static async Task<CaseDetails> GetCaseDetailsV1Async(IQueryBase queryBase, Uri caseUri)
        {
            return await queryBase.ProcessGetAsync<CaseDetails>(  // NOTE: CaseDetails v1
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseUri,  // Request URL
                fallbackErrorMessage: ZhvResources.HttpRequest_ERROR_NoCaseDetails);
        }
        #endregion
    }
}