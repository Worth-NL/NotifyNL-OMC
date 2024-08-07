﻿// © 2024, Worth Systems.

using EventsHandler.Exceptions;
using EventsHandler.Extensions;
using EventsHandler.Mapping.Models.POCOs.Objecten;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataSending.Clients.Enums;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Versioning.Interfaces;
using System.Text.Json;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenZaak.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    internal interface IQueryZaak : IVersionDetails, IDomain
    {
        /// <inheritdoc cref="WebApiConfiguration"/>
        protected internal WebApiConfiguration Configuration { get; set; }

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "OpenZaak";

        #region Parent
        /// <summary>
        /// Gets the <see cref="Case"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal sealed async Task<Case> GetCaseAsync(IQueryBase queryBase)
        {
            return await GetCaseAsync(queryBase, null);
        }

        /// <inheritdoc cref="GetCaseAsync(IQueryBase)"/>
        /// <exception cref="ArgumentException"/>
        internal sealed async Task<Case> GetCaseAsync(IQueryBase queryBase, Uri? caseTypeUrl)
        {
            if (caseTypeUrl != null && !caseTypeUrl.AbsoluteUri.Contains("/zaaktypen/"))
            {
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotCaseTypeUri);
            }

            caseTypeUrl ??= await GetCaseTypeUriAsync(queryBase, queryBase.Notification.MainObject);

            return await queryBase.ProcessGetAsync<Case>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseTypeUrl,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCase);
        }

        /// <inheritdoc cref="GetCaseAsync(IQueryBase)"/>
        internal sealed async Task<Case> GetCaseAsync(IQueryBase queryBase, Data taskData)
        {
            Uri caseTypeUri = await RequestCaseTypeUriAsync(queryBase, taskData.CaseUrl);

            return await GetCaseAsync(queryBase, caseTypeUri);
        }

        /// <inheritdoc cref="GetCaseAsync(IQueryBase)"/>
        internal sealed async Task<Case> GetCaseAsync(IQueryBase queryBase, Decision decision)
        {
            Uri caseTypeUri = await RequestCaseTypeUriAsync(queryBase, decision.CaseUrl);

            return await GetCaseAsync(queryBase, caseTypeUri);
        }

        /// <summary>
        /// Gets the status(es) of the specific <see cref="Case"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal sealed async Task<CaseStatuses> GetCaseStatusesAsync(IQueryBase queryBase)
        {
            // Predefined URL components
            string statusesEndpoint = $"https://{GetDomain()}/zaken/api/v1/statussen";

            // Request URL
            Uri caseStatuses = new($"{statusesEndpoint}?zaak={queryBase.Notification.MainObject}");

            return await queryBase.ProcessGetAsync<CaseStatuses>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseStatuses,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseStatuses);
        }

#pragma warning disable CA1822  // Method(s) can be marked as static but that would be inconsistent for interface
        /// <summary>
        /// Gets the type of <see cref="CaseStatus"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <exception cref="AbortedNotifyingException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal sealed async Task<CaseType> GetLastCaseTypeAsync(IQueryBase queryBase, CaseStatuses statuses)
        {
            // Request URL
            Uri lastStatusTypeUri = statuses.LastStatus().Type;

            return await queryBase.ProcessGetAsync<CaseType>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: lastStatusTypeUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseStatusType);
        }

        /// <summary>
        /// Gets the <see cref="MainObject"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal sealed async Task<MainObject> GetMainObjectAsync(IQueryBase queryBase)
        {
            return await queryBase.ProcessGetAsync<MainObject>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: queryBase.Notification.MainObject,  // Request URL
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoMainObject);
        }

        /// <summary>
        /// Gets the <see cref="Decision"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal sealed async Task<Decision> GetDecisionAsync(IQueryBase queryBase)
        {
            if (!queryBase.Notification.MainObject.AbsoluteUri.Contains("/besluiten/"))
            {
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotDecisionUri);
            }

            return await queryBase.ProcessGetAsync<Decision>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: queryBase.Notification.MainObject,  // Request URL
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoDecision);
        }
#pragma warning restore CA1822
        #endregion

        #region Abstract (BSN Number)
        /// <summary>
        /// Gets BSN number of a specific citizen from "OpenZaak" Web API service.
        /// </summary>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal Task<string> GetBsnNumberAsync(IQueryBase queryBase);

        /// <inheritdoc cref="GetBsnNumberAsync(IQueryBase)"/>
        internal Task<string> GetBsnNumberAsync(IQueryBase queryBase, Uri caseTypeUri);
        #endregion

        #region Abstract (Case type)
        /// <summary>
        /// Gets the callback <see cref="Uri"/> to obtain <see cref="Case"/> type from "OpenZaak" Web API service.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        private async Task<Uri> GetCaseTypeUriAsync(IQueryBase queryBase, Uri caseUri)
        {
            // Case type URI was already provided in the initial notification
            if (queryBase.Notification.Attributes.CaseType?.AbsoluteUri.IsNotEmpty() ?? false)
            {
                return queryBase.Notification.Attributes.CaseType;
            }

            // Main Object doesn't contain Case URI (e.g., the initial notification isn't a case scenario)
            if (!caseUri.AbsoluteUri.Contains("/zaken/"))
            {
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotCaseUri);
            }

            // Case type URI needs to be queried from Main Object
            return await RequestCaseTypeUriAsync(queryBase, caseUri);  // Fallback, providing case type URI anyway
        }

        /// <inheritdoc cref="GetCaseTypeUriAsync(IQueryBase, Uri)"/>
        protected Task<Uri> RequestCaseTypeUriAsync(IQueryBase queryBase, Uri caseUri);
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

        #region Polymorphic (Domain)
        /// <inheritdoc cref="IDomain.GetDomain"/>
        string IDomain.GetDomain() => this.Configuration.User.Domain.OpenZaak();
        #endregion
    }
}