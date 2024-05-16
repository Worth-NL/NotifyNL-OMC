// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak.v1;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Adapter
{
    /// <inheritdoc cref="IQueryContext"/>
    internal sealed class QueryContext : IQueryContext
    {
        private readonly WebApiConfiguration _configuration;

        private readonly IQueryBase _queryBase;
        private readonly IQueryKlant _queryKlant;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryContext"/> nested class.
        /// </summary>
        public QueryContext(WebApiConfiguration configuration, IQueryBase queryBase, IQueryKlant queryKlant)
        {
            this._configuration = configuration;

            // Composition
            this._queryBase = queryBase;
            this._queryKlant = queryKlant;
        }

        #region IQueryBase
        /// <inheritdoc cref="IQueryContext.SetNotification(NotificationEvent)"/>
        void IQueryContext.SetNotification(NotificationEvent notification)
        {
            this._queryBase.Notification = notification;
        }

        /// <inheritdoc cref="IQueryContext.ProcessPostAsync{TModel}(HttpClientTypes, Uri, HttpContent, string)"/>
        async Task<TModel> IQueryContext.ProcessPostAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, HttpContent body, string fallbackErrorMessage)
        {
            return await this._queryBase.ProcessPostAsync<TModel>(httpsClientType, uri, body, fallbackErrorMessage);
        }
        #endregion

        #region IQueryKlant
        /// <inheritdoc cref="IQueryContext.GetCitizenDetailsAsync()"/>
        async Task<CitizenDetails> IQueryContext.GetCitizenDetailsAsync()
        {
            // 1. Fetch BSN using "OpenZaak"
            string bsnNumber = await GetBsnNumberAsync();

            // 2. Fetch Citizen Details using "OpenKlant"
            return await this._queryKlant.GetCitizenDetailsAsync(this._queryBase, bsnNumber);
        }
        #endregion





        #region Internal query methods
        /// <inheritdoc cref="IQueryContext.GetCaseAsync()"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        async Task<Case> IQueryContext.GetCaseAsync()
        {
            return await this._queryBase.ProcessGetAsync<Case>(HttpClientTypes.Data, await GetCaseTypeAsync(), Resources.HttpRequest_ERROR_NoCase);
        }
        
        /// <inheritdoc cref="IQueryContext.GetCaseStatusesAsync()"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        async Task<CaseStatuses> IQueryContext.GetCaseStatusesAsync()
        {
            // Predefined URL components
            string statusesEndpoint = $"https://{GetSpecificOpenZaakDomain()}/zaken/api/v1/statussen";

            // Request URL
            Uri caseStatuses = new($"{statusesEndpoint}?zaak={this._queryBase.Notification.MainObject}");

            return await this._queryBase.ProcessGetAsync<CaseStatuses>(HttpClientTypes.Data, caseStatuses, Resources.HttpRequest_ERROR_NoCaseStatuses);
        }
        
        /// <inheritdoc cref="IQueryContext.GetLastCaseStatusTypeAsync(CaseStatuses)"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        async Task<CaseStatusType> IQueryContext.GetLastCaseStatusTypeAsync(CaseStatuses statuses)
        {
            // Request URL
            Uri lastStatusTypeUri = statuses.LastStatus().Type;

            return await this._queryBase.ProcessGetAsync<CaseStatusType>(HttpClientTypes.Data, lastStatusTypeUri, Resources.HttpRequest_ERROR_NoCaseStatusType);
        }
        #endregion

        #region Private query methods
        /// <summary>
        /// Gets the callback <see cref="Uri"/> to obtain <see cref="Case"/> type from "OpenZaak" Web service.
        /// </summary>
        private async Task<Uri> GetCaseTypeAsync()
        {
            return this._queryBase.Notification.Attributes.CaseType ?? (await GetCaseDetailsAsync()).CaseType;
        }

        /// <summary>
        /// Gets the <see cref="Case"/> details from "OpenZaak" Web service.
        /// </summary>
        private async Task<CaseDetails> GetCaseDetailsAsync()
        {
            return await this._queryBase.ProcessGetAsync<CaseDetails>(HttpClientTypes.Data, this._queryBase.Notification.MainObject, Resources.HttpRequest_ERROR_NoCaseDetails);
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
            Uri caseWithRoleUri = new($"{rolesEndpoint}?zaak={this._queryBase.Notification.MainObject}&betrokkeneType={roleType}");

            return await this._queryBase.ProcessGetAsync<CaseRoles>(HttpClientTypes.Data, caseWithRoleUri, Resources.HttpRequest_ERROR_NoCaseRole);
        }
        #endregion

        #region Helper methods
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