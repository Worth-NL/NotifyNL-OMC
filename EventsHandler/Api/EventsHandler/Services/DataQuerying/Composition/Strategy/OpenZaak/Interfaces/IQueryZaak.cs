// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "OpenZaak" Web service.
    /// </summary>
    internal interface IQueryZaak
    {
        /// <inheritdoc cref="WebApiConfiguration"/>
        protected internal WebApiConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets the <see cref="Case"/> from "OpenZaak" Web service.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        internal async Task<Case> GetCaseAsync(IQueryBase queryBase)
        {
            return await queryBase.ProcessGetAsync<Case>(
                httpsClientType: HttpClientTypes.Data,
                uri: await GetCaseTypeUriAsync(queryBase),
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCase);
        }
        
        /// <summary>
        /// Gets the status(es) of the specific <see cref="Case"/> from "OpenZaak" Web service.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        internal async Task<CaseStatuses> GetCaseStatusesAsync(IQueryBase queryBase)
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
        
        /// <summary>
        /// Gets the type of <see cref="CaseStatus"/> from "OpenZaak" Web service.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        internal async Task<CaseStatusType> GetLastCaseStatusTypeAsync(IQueryBase queryBase, CaseStatuses statuses)
        {
            // Request URL
            Uri lastStatusTypeUri = statuses.LastStatus().Type;

            return await queryBase.ProcessGetAsync<CaseStatusType>(
                httpsClientType: HttpClientTypes.Data,
                uri: lastStatusTypeUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseStatusType);
        }
        
        /// <summary>
        /// Gets BSN number of a specific citizen from "OpenZaak" Web service.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        internal async Task<string> GetBsnNumberAsync(IQueryBase queryBase)
        {
            // 1. Fetch case roles from "OpenZaak"
            // 2. Determine citizen data from case roles
            // 3. Return BSN from citizen data
            return (await GetCaseRoleAsync(queryBase)).Citizen.BsnNumber;
        }
        
        #region Helper methods
        /// <summary>
        /// Gets the callback <see cref="Uri"/> to obtain <see cref="Case"/> type from "OpenZaak" Web service.
        /// </summary>
        private async Task<Uri> GetCaseTypeUriAsync(IQueryBase queryBase)
        {
            return queryBase.Notification.Attributes.CaseTypeUri
                ?? await GetCaseTypeUriFromDetailsAsync(queryBase);  // Fallback, providing case type URI anyway
        }
        
        /// <summary>
        /// Gets the callback <see cref="Uri"/> to obtain <see cref="Case"/> type from "OpenZaak" Web service.
        /// </summary>
        protected Task<Uri> GetCaseTypeUriFromDetailsAsync(IQueryBase queryBase);

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
        private string GetSpecificOpenZaakDomain() => this.Configuration.User().Domain.OpenZaak();
        #endregion
    }
}