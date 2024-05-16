// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak.v1;
using EventsHandler.Configuration;
using EventsHandler.Extensions;
using EventsHandler.Services.DataQuerying.Strategy.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;
using EventsHandler.Services.DataReceiving.Interfaces;
using EventsHandler.Services.Serialization.Interfaces;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Strategy
{
    /// <inheritdoc cref="IQueryContext"/>
    internal sealed class QueryContext : IQueryContext
    {
        private readonly WebApiConfiguration _configuration;
        private readonly ISerializationService _serializer;
        private readonly IHttpSupplierService _httpSupplier;

        /// <inheritdoc cref="IQueryContext.Notification"/>
        NotificationEvent IQueryContext.Notification { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiDataQuery"/> nested class.
        /// </summary>
        public QueryContext(
            WebApiConfiguration configuration,
            ISerializationService serializer,
            IHttpSupplierService httpSupplier)
        {
            this._configuration = configuration;
            this._serializer = serializer;
            this._httpSupplier = httpSupplier;
        }

        #region General HTTP methods
        /// <inheritdoc cref="IQueryContext.ProcessGetAsync{TModel}(HttpClientTypes, Uri, string)"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        async Task<TModel> IQueryContext.ProcessGetAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, string fallbackErrorMessage)
        {
            string organizationId = ((IQueryContext)this).Notification.GetOrganizationId();

            (bool isSuccess, string jsonResult) = await this._httpSupplier.GetAsync(httpsClientType, organizationId, uri);

            return GetApiResult<TModel>(isSuccess, jsonResult, uri, fallbackErrorMessage);
        }

        /// <inheritdoc cref="IQueryContext.ProcessPostAsync{TModel}(HttpClientTypes, Uri, HttpContent, string)"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        async Task<TModel> IQueryContext.ProcessPostAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, HttpContent body, string fallbackErrorMessage)
        {
            string organizationId = ((IQueryContext)this).Notification.GetOrganizationId();

            (bool isSuccess, string jsonResult) = await this._httpSupplier.PostAsync(httpsClientType, organizationId, uri, body);

            return GetApiResult<TModel>(isSuccess, jsonResult, uri, fallbackErrorMessage);
        }

        private TModel GetApiResult<TModel>(bool isSuccess, string jsonResult, Uri uri, string fallbackErrorMessage)
            where TModel : struct, IJsonSerializable
        {
            return isSuccess ? this._serializer.Deserialize<TModel>(jsonResult)
                             : throw new HttpRequestException($"{fallbackErrorMessage} | URI: {uri} | JSON response: {jsonResult}");
        }
        #endregion

        #region Internal query methods
        /// <inheritdoc cref="IQueryContext.GetCaseAsync()"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        async Task<Case> IQueryContext.GetCaseAsync()
        {
            return await ((IQueryContext)this).ProcessGetAsync<Case>(HttpClientTypes.Data, await GetCaseTypeAsync(), Resources.HttpRequest_ERROR_NoCase);
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

            return await ((IQueryContext)this).ProcessGetAsync<CitizenDetails>(HttpClientTypes.Data, citizenByBsnUri, Resources.HttpRequest_ERROR_NoCitizenDetails);
        }
        
        /// <inheritdoc cref="IQueryContext.GetCaseStatusesAsync()"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        async Task<CaseStatuses> IQueryContext.GetCaseStatusesAsync()
        {
            // Predefined URL components
            string statusesEndpoint = $"https://{GetSpecificOpenZaakDomain()}/zaken/api/v1/statussen";

            // Request URL
            Uri caseStatuses = new($"{statusesEndpoint}?zaak={((IQueryContext)this).Notification.MainObject}");

            return await ((IQueryContext)this).ProcessGetAsync<CaseStatuses>(HttpClientTypes.Data, caseStatuses, Resources.HttpRequest_ERROR_NoCaseStatuses);
        }
        
        /// <inheritdoc cref="IQueryContext.GetLastCaseStatusTypeAsync(CaseStatuses)"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        async Task<CaseStatusType> IQueryContext.GetLastCaseStatusTypeAsync(CaseStatuses statuses)
        {
            // Request URL
            Uri lastStatusTypeUri = statuses.LastStatus().Type;

            return await ((IQueryContext)this).ProcessGetAsync<CaseStatusType>(HttpClientTypes.Data, lastStatusTypeUri, Resources.HttpRequest_ERROR_NoCaseStatusType);
        }
        #endregion

        #region Private query methods
        /// <summary>
        /// Gets the callback <see cref="Uri"/> to obtain <see cref="Case"/> type from "OpenZaak" Web service.
        /// </summary>
        private async Task<Uri> GetCaseTypeAsync()
        {
            return ((IQueryContext)this).Notification.Attributes.CaseType ?? (await GetCaseDetailsAsync()).CaseType;
        }

        /// <summary>
        /// Gets the <see cref="Case"/> details from "OpenZaak" Web service.
        /// </summary>
        private async Task<CaseDetails> GetCaseDetailsAsync()
        {
            return await ((IQueryContext)this).ProcessGetAsync<CaseDetails>(HttpClientTypes.Data, ((IQueryContext)this).Notification.MainObject, Resources.HttpRequest_ERROR_NoCaseDetails);
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
            Uri caseWithRoleUri = new($"{rolesEndpoint}?zaak={((IQueryContext)this).Notification.MainObject}&betrokkeneType={roleType}");

            return await ((IQueryContext)this).ProcessGetAsync<CaseRoles>(HttpClientTypes.Data, caseWithRoleUri, Resources.HttpRequest_ERROR_NoCaseRole);
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