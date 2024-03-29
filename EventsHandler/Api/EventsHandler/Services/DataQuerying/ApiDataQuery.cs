﻿// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
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
        
        private QueryContext? _notificationBuilder;

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
        QueryContext IDataQueryService<NotificationEvent>.From(NotificationEvent notification)
        {
            // To optimize the workflow keep the notification builder cached
            if (this._notificationBuilder == null)
            {
                return this._notificationBuilder ??=
                    new QueryContext(this._httpSupplier.Configuration, this._serializer, this._httpSupplier, notification);
            }

            // Update only the current notification in cached builder
            this._notificationBuilder.Notification = notification;

            return this._notificationBuilder;
        }

        /// <summary>
        /// The nested builder operating on <see cref="NotificationEvent"/>.
        /// </summary>
        internal sealed class QueryContext  // TODO: Introduce service "IQueryContext" here
        {
            private readonly WebApiConfiguration _configuration;
            private readonly ISerializationService _serializer;
            private readonly IHttpSupplierService _httpSupplier;

            /// <summary>
            /// The notification from "Notificatie API" Web service.
            /// </summary>
            internal NotificationEvent Notification { private get; set; }

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
            /// Sends the <see cref="HttpMethods.Get"/> request to the specified URI and deserializes received JSON result.
            /// </summary>
            /// <exception cref="InvalidOperationException"/>
            /// <exception cref="HttpRequestException"/>
            internal async Task<TModel> ProcessGetAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, string fallbackErrorMessage)
                where TModel : struct, IJsonSerializable
            {
                string organizationId = this.Notification.GetOrganizationId();

                (bool isSuccess, string jsonResult) = await this._httpSupplier.GetAsync(httpsClientType, organizationId, uri);

                return isSuccess ? this._serializer.Deserialize<TModel>(jsonResult)
                                 : throw new HttpRequestException($"{fallbackErrorMessage} | URI: {uri} | JSON response: {jsonResult}");
            }

            /// <summary>
            /// Sends the <see cref="HttpMethods.Post"/> request to the specified URI and deserializes received JSON result.
            /// </summary>
            /// <exception cref="InvalidOperationException"/>
            /// <exception cref="HttpRequestException"/>
            internal async Task<TModel> ProcessPostAsync<TModel>(HttpClientTypes httpsClientType, Uri uri, HttpContent body, string fallbackErrorMessage)
                where TModel : struct, IJsonSerializable
            {
                string organizationId = this.Notification.GetOrganizationId();

                (bool isSuccess, string jsonResult) = await this._httpSupplier.PostAsync(httpsClientType, organizationId, uri, body);

                return isSuccess ? this._serializer.Deserialize<TModel>(jsonResult)
                                 : throw new HttpRequestException($"{fallbackErrorMessage} | URI: {uri} | JSON response: {jsonResult}");
            }

            #region Internal query methods
            /// <summary>
            /// Gets the <see cref="MainObject"/> from "OpenZaak" Web service.
            /// </summary>
            internal async Task<MainObject> GetMainObjectAsync()  // TODO: This method is not yet used
            {
                return await ProcessGetAsync<MainObject>(HttpClientTypes.Data, this.Notification.MainObject, Resources.HttpRequest_ERROR_NoMainObject);
            }

            /// <summary>
            /// Gets the <see cref="Case"/> from "OpenZaak" Web service.
            /// </summary>
            internal async Task<Case> GetCaseAsync()
            {
                return await ProcessGetAsync<Case>(HttpClientTypes.Data, await GetCaseTypeAsync(), Resources.HttpRequest_ERROR_NoCase);
            }

            /// <summary>
            /// Gets the details of a specific citizen from "OpenKlant" Web service.
            /// </summary>
            internal async Task<CitizenDetails> GetCitizenDetailsAsync()
            {
                // Predefined URL components
                string citizensEndpoint = $"https://{GetSpecificOpenKlantDomain()}/klanten/api/v1/klanten";

                // Request URL
                Uri citizenByBsnUri = new($"{citizensEndpoint}?subjectNatuurlijkPersoon__inpBsn={await GetBsnNumberAsync()}");

                return await ProcessGetAsync<CitizenDetails>(HttpClientTypes.Data, citizenByBsnUri, Resources.HttpRequest_ERROR_NoCitizenDetails);
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

                return await ProcessGetAsync<CaseStatuses>(HttpClientTypes.Data, caseStatuses, Resources.HttpRequest_ERROR_NoCaseStatuses);
            }

            /// <summary>
            /// Gets the type of <see cref="CaseStatus"/>.
            /// </summary>
            internal async Task<CaseStatusType> GetLastCaseStatusTypeAsync(CaseStatuses statuses)
            {
                // Request URL
                Uri lastStatusTypeUri = statuses.LastStatus().Type;

                return await ProcessGetAsync<CaseStatusType>(HttpClientTypes.Data, lastStatusTypeUri, Resources.HttpRequest_ERROR_NoCaseStatusType);
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
                return await ProcessGetAsync<CaseDetails>(HttpClientTypes.Data, this.Notification.MainObject, Resources.HttpRequest_ERROR_NoCaseDetails);
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

                return await ProcessGetAsync<CaseRoles>(HttpClientTypes.Data, caseWithRoleUri, Resources.HttpRequest_ERROR_NoCaseRole);
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