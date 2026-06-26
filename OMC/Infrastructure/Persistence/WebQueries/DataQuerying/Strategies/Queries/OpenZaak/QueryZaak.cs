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
        string IVersionDetails.Version => "1.12.1";

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
