// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak.v1;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.v1
{
    /// <inheritdoc cref="IQueryZaak"/>
    internal sealed class QueryZaak : IQueryZaak
    {
        private readonly WebApiConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryZaak"/> class.
        /// </summary>
        public QueryZaak(WebApiConfiguration configuration)
        {
            this._configuration = configuration;
        }

        #region Internal methods
        /// <inheritdoc cref="IQueryZaak.GetCaseAsync(IQueryBase)"/>
        async Task<Case> IQueryZaak.GetCaseAsync(IQueryBase queryBase)
        {
            return await queryBase.ProcessGetAsync<Case>(
                httpsClientType: HttpClientTypes.Data,
                uri: await GetCaseTypeAsync(queryBase),
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCase);
        }

        /// <inheritdoc cref="IQueryZaak.GetCaseStatusesAsync(IQueryBase)"/>
        async Task<CaseStatuses> IQueryZaak.GetCaseStatusesAsync(IQueryBase queryBase)
        {
            // Predefined URL components
            string statusesEndpoint = $"https://{GetSpecificOpenZaakDomain()}/zaken/api/v1/statussen";

            // Request URL
            Uri caseStatuses = new($"{statusesEndpoint}?zaak={queryBase.Notification.MainObject}");

            return await queryBase.ProcessGetAsync<CaseStatuses>(
                httpsClientType: HttpClientTypes.Data,
                uri: caseStatuses,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseStatuses);
        }
        
        /// <inheritdoc cref="IQueryZaak.GetLastCaseStatusTypeAsync(IQueryBase, CaseStatuses)"/>
        async Task<CaseStatusType> IQueryZaak.GetLastCaseStatusTypeAsync(IQueryBase queryBase, CaseStatuses statuses)
        {
            // Request URL
            Uri lastStatusTypeUri = statuses.LastStatus().Type;

            return await queryBase.ProcessGetAsync<CaseStatusType>(
                httpsClientType: HttpClientTypes.Data,
                uri: lastStatusTypeUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseStatusType);
        }

        /// <inheritdoc cref="IQueryZaak.GetBsnNumberAsync(IQueryBase)"/>
        async Task<string> IQueryZaak.GetBsnNumberAsync(IQueryBase queryBase)
        {
            // 1. Fetch case roles from "OpenZaak"
            // 2. Determine citizen data from case roles
            // 3. Return BSN from citizen data
            return (await GetCaseRoleAsync(queryBase)).Citizen.BsnNumber;
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Gets the callback <see cref="Uri"/> to obtain <see cref="Case"/> type from "OpenZaak" Web service.
        /// </summary>
        private static async Task<Uri> GetCaseTypeAsync(IQueryBase queryBase)
        {
            return queryBase.Notification.Attributes.CaseType
                ?? (await GetCaseDetailsAsync(queryBase)).CaseType;  // Fallback, providing CaseType anyway
        }
        
        /// <summary>
        /// Gets the <see cref="Case"/> details from "OpenZaak" Web service.
        /// </summary>
        private static async Task<CaseDetails> GetCaseDetailsAsync(IQueryBase queryBase)
        {
            return await queryBase.ProcessGetAsync<CaseDetails>(
                httpsClientType: HttpClientTypes.Data,
                uri: queryBase.Notification.MainObject,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseDetails);
        }

        /// <summary>
        /// Gets the <see cref="Case"/> role from "OpenZaak" Web service.
        /// </summary>
        private async Task<CaseRoles> GetCaseRoleAsync(IQueryBase queryBase)
        {
            // Predefined URL components
            string rolesEndpoint = $"https://{GetSpecificOpenZaakDomain()}/zaken/api/v1/rollen";
            const string roleType = "natuurlijk_persoon";

            // Request URL
            Uri caseWithRoleUri = new($"{rolesEndpoint}?zaak={queryBase.Notification.MainObject}&betrokkeneType={roleType}");

            return await queryBase.ProcessGetAsync<CaseRoles>(
                httpsClientType: HttpClientTypes.Data,
                uri: caseWithRoleUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseRole);
        }

        /// <summary>
        /// Gets the domain part of the organization-specific (e.g., municipality) "OpenZaak" Web service URI.
        /// <para>
        ///   <code>http(s)://[DOMAIN]/ApiEndpoint</code>
        /// </para>
        /// </summary>
        private string GetSpecificOpenZaakDomain() => this._configuration.User.Domain.OpenZaak();
        #endregion
    }
}