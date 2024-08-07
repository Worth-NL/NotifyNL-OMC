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
        /// <exception cref="ArgumentException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal sealed async Task<Case> GetCaseAsync(IQueryBase queryBase, object? parameter = null)
        {
            Uri? caseTypeUri = null;

            // Case #1: The case type URI isn't provided so, it needs to be re-queried
            if (parameter == null)
            {
                caseTypeUri = await TryGetCaseTypeUriAsync(queryBase, queryBase.Notification.MainObject);
        }

            // Case #2: The URI was provided (but it might be the incorrect one)
            if (parameter is Uri uri)
        {
                if (!uri.AbsoluteUri.Contains("/zaaktypen/"))  // Needs to be the case type URI
            {
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotCaseTypeUri);
            }

                caseTypeUri = uri;
        }

            // Case #3: The case type URI can be requested from Data
            if (parameter is Data taskData)
        {
                caseTypeUri = await GetCaseTypeUriAsync(queryBase, taskData.CaseUrl);
        }

            return await queryBase.ProcessGetAsync<Case>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseTypeUri ?? throw new ArgumentException(Resources.Operation_ERROR_Internal_NotCaseTypeUri),
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCase);
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
            Uri caseStatuses = new($"{statusesEndpoint}?zaak={queryBase.Notification.MainObject}");  // TODO: Main Object validation

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
                uri: queryBase.Notification.MainObject,  // Decision URL
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
        private async Task<Uri> TryGetCaseTypeUriAsync(IQueryBase queryBase, Uri caseUri)
        {
            // Case #1: The case type URI was already provided in the initial notification
            if (queryBase.Notification.Attributes.CaseType?.AbsoluteUri.IsNotEmpty() ?? false)
            {
                return queryBase.Notification.Attributes.CaseType;
            }

            // Case #2: The Main Object doesn't contain case URI (e.g., the initial notification isn't a case scenario)
            if (!caseUri.AbsoluteUri.Contains("/zaken/"))
            {
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotCaseUri);
            }

            // Case #3: The case type URI needs to be queried from the Main Object
            return await GetCaseTypeUriAsync(queryBase, caseUri);  // Fallback, providing case type URI anyway
        }

        /// <inheritdoc cref="TryGetCaseTypeUriAsync"/>
        protected Task<Uri> GetCaseTypeUriAsync(IQueryBase queryBase, Uri caseUri);
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