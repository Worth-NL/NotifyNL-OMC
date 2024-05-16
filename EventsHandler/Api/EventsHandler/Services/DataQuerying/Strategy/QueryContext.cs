// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak.v1;
using EventsHandler.Configuration;
using EventsHandler.Extensions;
using EventsHandler.Services.DataQuerying.Strategy.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Strategy
{
    /// <inheritdoc cref="IQueryContext"/>
    internal sealed class QueryContext : IQueryContext
    {
        private readonly WebApiConfiguration _configuration;

        /// <inheritdoc cref="IQueryContext.QueryBase"/>
        IQueryBase IQueryContext.QueryBase { get; set; } = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryContext"/> nested class.
        /// </summary>
        public QueryContext(WebApiConfiguration configuration, IQueryBase queryBase)
        {
            this._configuration = configuration;

            // Composition
            ((IQueryContext)this).QueryBase = queryBase;
        }

        #region Internal query methods
        /// <inheritdoc cref="IQueryContext.GetCaseAsync()"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        async Task<Case> IQueryContext.GetCaseAsync()
        {
            return await ((IQueryContext)this).QueryBase.ProcessGetAsync<Case>(HttpClientTypes.Data, await GetCaseTypeAsync(), Resources.HttpRequest_ERROR_NoCase);
        }

        /// <inheritdoc cref="IQueryContext.GetCitizenDetailsAsync()"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        async Task<CitizenDetails> IQueryContext.GetCitizenDetailsAsync()
        {
            // Predefined URL components
            string citizensEndpoint;
            
            // Request URL
            Uri citizenByBsnUri;

            if (!this._configuration.AppSettings.UseNewOpenKlant())
            {
                // Open Klant 1.0
                citizensEndpoint = $"https://{GetSpecificOpenKlantDomain()}/klanten/api/v1/klanten";
                citizenByBsnUri = new Uri($"{citizensEndpoint}?subjectNatuurlijkPersoon__inpBsn={await GetBsnNumberAsync()}");
            }
            else
            {
                // Open Klant 2.0
                citizensEndpoint = $"https://{GetSpecificOpenKlantDomain()}/";  // TODO: To be finished
                citizenByBsnUri = new Uri(citizensEndpoint);                    // TODO: To be finished
            }

            return await ((IQueryContext)this).QueryBase.ProcessGetAsync<CitizenDetails>(HttpClientTypes.Data, citizenByBsnUri, Resources.HttpRequest_ERROR_NoCitizenDetails);
        }
        
        /// <inheritdoc cref="IQueryContext.GetCaseStatusesAsync()"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        async Task<CaseStatuses> IQueryContext.GetCaseStatusesAsync()
        {
            // Predefined URL components
            string statusesEndpoint = $"https://{GetSpecificOpenZaakDomain()}/zaken/api/v1/statussen";

            // Request URL
            Uri caseStatuses = new($"{statusesEndpoint}?zaak={((IQueryContext)this).QueryBase.Notification.MainObject}");

            return await ((IQueryContext)this).QueryBase.ProcessGetAsync<CaseStatuses>(HttpClientTypes.Data, caseStatuses, Resources.HttpRequest_ERROR_NoCaseStatuses);
        }
        
        /// <inheritdoc cref="IQueryContext.GetLastCaseStatusTypeAsync(CaseStatuses)"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        async Task<CaseStatusType> IQueryContext.GetLastCaseStatusTypeAsync(CaseStatuses statuses)
        {
            // Request URL
            Uri lastStatusTypeUri = statuses.LastStatus().Type;

            return await ((IQueryContext)this).QueryBase.ProcessGetAsync<CaseStatusType>(HttpClientTypes.Data, lastStatusTypeUri, Resources.HttpRequest_ERROR_NoCaseStatusType);
        }
        #endregion

        #region Private query methods
        /// <summary>
        /// Gets the callback <see cref="Uri"/> to obtain <see cref="Case"/> type from "OpenZaak" Web service.
        /// </summary>
        private async Task<Uri> GetCaseTypeAsync()
        {
            return ((IQueryContext)this).QueryBase.Notification.Attributes.CaseType ?? (await GetCaseDetailsAsync()).CaseType;
        }

        /// <summary>
        /// Gets the <see cref="Case"/> details from "OpenZaak" Web service.
        /// </summary>
        private async Task<CaseDetails> GetCaseDetailsAsync()
        {
            return await ((IQueryContext)this).QueryBase.ProcessGetAsync<CaseDetails>(HttpClientTypes.Data, ((IQueryContext)this).QueryBase.Notification.MainObject, Resources.HttpRequest_ERROR_NoCaseDetails);
        }

        /// <summary>
        /// Gets BSN number of a specific citizen from "OpenZaak" Web service.
        /// </summary>
        private async Task<string> GetBsnNumberAsync() => (await GetCaseRoleAsync()).Citizen.BsnNumber;

        /// <summary>
        /// Gets the <see cref="Case"/> role from "OpenZaak" Web service.
        /// </summary>
        private async Task<CaseRoles> GetCaseRoleAsync()
        {
            // Predefined URL components
            string rolesEndpoint = $"https://{GetSpecificOpenZaakDomain()}/zaken/api/v1/rollen";
            const string roleType = "natuurlijk_persoon";

            // Request URL
            Uri caseWithRoleUri = new($"{rolesEndpoint}?zaak={((IQueryContext)this).QueryBase.Notification.MainObject}&betrokkeneType={roleType}");

            return await ((IQueryContext)this).QueryBase.ProcessGetAsync<CaseRoles>(HttpClientTypes.Data, caseWithRoleUri, Resources.HttpRequest_ERROR_NoCaseRole);
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Gets the domain part of the organization-specific (municipality) "OpenZaak" URI.
        /// <para>
        ///   <code>http(s):// [DOMAIN] /ApiEndpoint</code>
        /// </para>
        /// </summary>
        private string GetSpecificOpenZaakDomain() => this._configuration.User.Domain.OpenZaak();

        /// <summary>
        /// Gets the domain part of the organization-specific (municipality) "OpenKlant" URI.
        /// <para>
        ///   <code>http(s):// [DOMAIN] /ApiEndpoint</code>
        /// </para>
        /// </summary>
        private string GetSpecificOpenKlantDomain() => this._configuration.User.Domain.OpenKlant();
        #endregion
    }
}