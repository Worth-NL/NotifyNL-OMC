// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Configuration;
using EventsHandler.Extensions;
using EventsHandler.Properties;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;
using EventsHandler.Services.DataReceiving.Interfaces;
using EventsHandler.Services.Serialization.Interfaces;

namespace EventsHandler.Services.DataQuerying
{
    /// <inheritdoc cref="IDataQueryService{TModel}"/>
    internal sealed class ApiDataQuery : IDataQueryService<NotificationEvent>
    {
        private readonly ISerializationService _serializer;
        private readonly IHttpSupplierService _httpSupplier;
        
        private IQueryContext? _queryContext;

        /// <inheritdoc cref="IDataQueryService{TModel}.HttpSupplier"/>
        IHttpSupplierService IDataQueryService<NotificationEvent>.HttpSupplier => this._httpSupplier;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiDataQuery"/> class.
        /// </summary>
        public ApiDataQuery(
            ISerializationService serializer,
            IHttpSupplierService httpSupplier)
        {
            this._serializer = serializer;
            this._httpSupplier = httpSupplier;
        }

        /// <inheritdoc cref="IDataQueryService{TModel}.From(TModel)"/>
        IQueryContext IDataQueryService<NotificationEvent>.From(NotificationEvent notification)
        {
            // To optimize the workflow keep the notification builder cached
            if (this._queryContext == null)
            {
                return this._queryContext ??=
                    new QueryContext(this._httpSupplier.Configuration, this._serializer, this._httpSupplier, notification);
            }

            // Update only the current notification in cached builder
            this._queryContext.Notification = notification;

            return this._queryContext;
        }

        /// <inheritdoc cref="IQueryContext"/>
        internal sealed class QueryContext : IQueryContext
        {
            private readonly WebApiConfiguration _configuration;
            private readonly ISerializationService _serializer;
            private readonly IHttpSupplierService _httpSupplier;

            /// <inheritdoc cref="IQueryContext.Notification"/>
            public NotificationEvent Notification { internal get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ApiDataQuery"/> nested class.
            /// </summary>
            internal QueryContext(
                WebApiConfiguration configuration,
                ISerializationService serializer,
                IHttpSupplierService httpSupplier,
                NotificationEvent notification)
            {
                this._configuration = configuration;
                this._serializer = serializer;
                this._httpSupplier = httpSupplier;

                this.Notification = notification;
            }

            /// <summary>
            ///   <inheritdoc cref="IQueryContext.ProcessGetAsync{TModel}(HttpClientTypes, Uri, string)"/>
            /// </summary>
            /// <exception cref="InvalidOperationException"/>
            /// <exception cref="HttpRequestException"/>
            async Task<TModel> IQueryContext.ProcessGetAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, string fallbackErrorMessage)
            {
                string organizationId = this.Notification.GetOrganizationId();

                (bool isSuccess, string jsonResult) = await this._httpSupplier.GetAsync(httpsClientType, organizationId, uri);

                return isSuccess ? this._serializer.Deserialize<TModel>(jsonResult)
                                 : throw new HttpRequestException($"{fallbackErrorMessage} | URI: {uri} | JSON response: {jsonResult}");
            }

            /// <summary>
            ///   <inheritdoc cref="IQueryContext.ProcessPostAsync{TModel}(HttpClientTypes, Uri, HttpContent, string)"/>
            /// </summary>
            /// <exception cref="InvalidOperationException"/>
            /// <exception cref="HttpRequestException"/>
            async Task<TModel> IQueryContext.ProcessPostAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, HttpContent body, string fallbackErrorMessage)
            {
                string organizationId = this.Notification.GetOrganizationId();

                (bool isSuccess, string jsonResult) = await this._httpSupplier.PostAsync(httpsClientType, organizationId, uri, body);

                return isSuccess ? this._serializer.Deserialize<TModel>(jsonResult)
                                 : throw new HttpRequestException($"{fallbackErrorMessage} | URI: {uri} | JSON response: {jsonResult}");
            }

            #region Internal query methods
            /// <summary>
            /// Gets the <see cref="Case"/> from "OpenZaak" Web service.
            /// </summary>
            internal async Task<Case> GetCaseAsync()
            {
                return await ((IQueryContext)this).ProcessGetAsync<Case>(HttpClientTypes.Data, await GetCaseTypeAsync(), Resources.HttpRequest_ERROR_NoCase);
            }

            /// <summary>
            /// Gets the details of a specific citizen from "OpenKlant" Web service.
            /// </summary>
            internal async Task<CitizenDetails> GetCitizenDetailsAsync()
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

            /// <summary>
            /// Gets the status(es) of the specific <see cref="Case"/> from "OpenZaak" Web service.
            /// </summary>
            internal async Task<CaseStatuses> GetCaseStatusesAsync()
            {
                // Predefined URL components
                string statusesEndpoint = $"https://{GetSpecificOpenZaakDomain()}/zaken/api/v1/statussen";

                // Request URL
                Uri caseStatuses = new($"{statusesEndpoint}?zaak={this.Notification.MainObject}");

                return await ((IQueryContext)this).ProcessGetAsync<CaseStatuses>(HttpClientTypes.Data, caseStatuses, Resources.HttpRequest_ERROR_NoCaseStatuses);
            }

            /// <summary>
            /// Gets the type of <see cref="CaseStatus"/>.
            /// </summary>
            internal async Task<CaseStatusType> GetLastCaseStatusTypeAsync(CaseStatuses statuses)
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
                return this.Notification.Attributes.CaseType ?? (await GetCaseDetailsAsync()).CaseType;
            }

            /// <summary>
            /// Gets the <see cref="Case"/> details from "OpenZaak" Web service.
            /// </summary>
            private async Task<CaseDetails> GetCaseDetailsAsync()
            {
                return await ((IQueryContext)this).ProcessGetAsync<CaseDetails>(HttpClientTypes.Data, this.Notification.MainObject, Resources.HttpRequest_ERROR_NoCaseDetails);
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
                Uri caseWithRoleUri = new($"{rolesEndpoint}?zaak={this.Notification.MainObject}&betrokkeneType={roleType}");

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
}