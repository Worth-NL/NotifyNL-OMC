// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Behaviors.Versioning;
using EventsHandler.Configuration;
using EventsHandler.Exceptions;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;
using EventsHandler.Services.DataReceiving.Interfaces;
using System.Text.Json;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    internal interface IQueryZaak : IVersionDetails
    {
        /// <inheritdoc cref="WebApiConfiguration"/>
        protected internal WebApiConfiguration Configuration { get; set; }

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "OpenZaak";

        /// <summary>
        /// Gets the <see cref="Case"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <exception cref="JsonException"/>
        /// <exception cref="HttpRequestException"/>
        internal sealed async Task<Case> GetCaseAsync(IQueryBase queryBase)
        {
            return await queryBase.ProcessGetAsync<Case>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: await GetCaseTypeUriAsync(queryBase),
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCase);
        }
        
        /// <summary>
        /// Gets the status(es) of the specific <see cref="Case"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="JsonException"/>
        /// <exception cref="HttpRequestException"/>
        internal sealed async Task<CaseStatuses> GetCaseStatusesAsync(IQueryBase queryBase)
        {
            // Predefined URL components
            string statusesEndpoint = $"https://{GetSpecificOpenZaakDomain()}/zaken/api/v1/statussen";

            // Request URL
            Uri caseStatuses = new($"{statusesEndpoint}?zaak={queryBase.Notification.MainObject}");

            return await queryBase.ProcessGetAsync<CaseStatuses>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseStatuses,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseStatuses);
        }
        
        /// <summary>
        /// Gets the type of <see cref="CaseStatus"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <exception cref="JsonException"/>
        /// <exception cref="HttpRequestException"/>
        #pragma warning disable CA1822  // Method can be marked as static but that would be inconsistent for interface
        internal sealed async Task<CaseStatusType> GetLastCaseStatusTypeAsync(IQueryBase queryBase, CaseStatuses statuses)
        #pragma warning restore CA1822
        {
            // Request URL
            Uri lastStatusTypeUri = statuses.LastStatus().Type;

            return await queryBase.ProcessGetAsync<CaseStatusType>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: lastStatusTypeUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseStatusType);
        }

        /// <summary>
        /// Gets the <see cref="MainObject"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <exception cref="JsonException"/>
        /// <exception cref="HttpRequestException"/>
        internal async Task<MainObject> GetMainObjectAsync(IQueryBase queryBase)
        {
            return await queryBase.ProcessGetAsync<MainObject>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: queryBase.Notification.MainObject,  // Request URL
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoMainObject);
        }

        #region Abstract (BSN Number)
        /// <summary>
        /// Gets BSN number of a specific citizen from "OpenZaak" Web API service.
        /// </summary>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="JsonException"/>
        /// <exception cref="HttpRequestException"/>
        internal Task<string> GetBsnNumberAsync(IQueryBase queryBase);
        #endregion

        #region Abstract (Case type)
        /// <summary>
        /// Gets the callback <see cref="Uri"/> to obtain <see cref="Case"/> type from "OpenZaak" Web API service.
        /// </summary>
        /// <exception cref="JsonException"/>
        /// <exception cref="HttpRequestException"/>
        private async Task<Uri> GetCaseTypeUriAsync(IQueryBase queryBase)
        {
            return queryBase.Notification.Attributes.CaseTypeUri
                ?? await GetCaseTypeUriFromDetailsAsync(queryBase);  // Fallback, providing case type URI anyway
        }

        /// <inheritdoc cref="GetCaseTypeUriAsync(IQueryBase)"/>
        protected Task<Uri> GetCaseTypeUriFromDetailsAsync(IQueryBase queryBase);
        #endregion

        #region Abstract (Telemetry)
        /// <summary>
        /// Sends the completion feedback to "OpenZaak" Web API service.
        /// </summary>
        /// <returns>
        ///   The JSON response from an external Telemetry Web API service.
        /// </returns>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="TelemetryException"/>
        internal Task<string> SendFeedbackAsync(IHttpNetworkService networkService, HttpContent body);
        #endregion

        #region Domain
        /// <summary>
        /// Gets the domain part of the organization-specific (e.g., municipality) "OpenZaak" Web API service URI:
        /// <code>
        ///   http(s)://[DOMAIN]/ApiEndpoint
        /// </code>
        /// </summary>
        /// <exception cref="KeyNotFoundException"/>
        internal sealed string GetSpecificOpenZaakDomain() => this.Configuration.User.Domain.OpenZaak();
        #endregion
    }
}