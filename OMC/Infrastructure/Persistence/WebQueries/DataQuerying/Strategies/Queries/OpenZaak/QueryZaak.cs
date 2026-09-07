// © 2024, Worth Systems.

using Common.Settings.Configuration;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenZaak.Interfaces;
using WebQueries.DataSending.Clients.Enums;
using WebQueries.Versioning.Interfaces;
using ZgwModels.Mapping.Models.POCOs.OpenZaak;
using ZgwModels.Properties;

namespace WebQueries.DataQuerying.Strategies.Queries.OpenZaak
{
    /// <inheritdoc cref="IQueryZaak"/>
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
            // Predefined URL components
            string rolesEndpoint = $"{((IQueryZaak)this).GetDomain()}/rollen";

            // Request URL
            var caseWithRoleUri = new Uri($"{rolesEndpoint}?zaak={caseUri}");

            CaseRoles caseRoles = await queryBase.ProcessGetAsync<CaseRoles>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseWithRoleUri,
                fallbackErrorMessage: ZgwResources.HttpRequest_ERROR_NoCaseRole);

            return caseRoles.CaseRole(((IQueryZaak)this).Configuration);
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
                "betrokkeneType=natuurlijk_persoon"
            };
            string query = string.Join("&", queryParams);
            var requestUri = new Uri($"{rolesEndpoint}?{query}");

            CaseRoles response = await queryBase.ProcessGetAsync<CaseRoles>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: requestUri,
                fallbackErrorMessage: ZgwResources.HttpRequest_ERROR_NoCaseRole);

            // Since Results is never null (initialized as empty list), we can simply:
            return response.Results.Any();
        }
        #endregion

        #region Polymorphic (Case type URI)
        /// <inheritdoc cref="IQueryZaak.GetCaseTypeUriAsync(IQueryBase, Uri)"/>
        async Task<Uri> IQueryZaak.GetCaseTypeUriAsync(IQueryBase queryBase, Uri caseUri)
        {
            CaseDetails caseDetails = await queryBase.ProcessGetAsync<CaseDetails>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseUri,
                fallbackErrorMessage: ZgwResources.HttpRequest_ERROR_NoCaseDetails);

            return caseDetails.CaseTypeUrl;
        }
        #endregion
    }
}
