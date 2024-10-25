// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataSending.Clients.Enums;
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
    /// <seealso cref="IDomain"/>
    internal interface IQueryZaak : IVersionDetails, IDomain
    {
        /// <inheritdoc cref="WebApiConfiguration"/>
        protected internal WebApiConfiguration Configuration { get; set; }

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "OpenZaak";

        #region Parent (Case)
        #pragma warning disable CA1822  // Method(s) can be marked as static but that would be inconsistent for interface
        /// <summary>
        /// Gets the <see cref="Case"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="caseUri">
        ///   <list type="number">
        ///     <item>Nothing => 2. Case <see cref="Uri"/> will be queried from the Main Object URI</item>
        ///     <item>Case <see cref="Uri"/> => to be used directly</item>
        ///   </list>
        /// </param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal sealed async Task<Case> TryGetCaseAsync(IQueryBase queryBase, Uri? caseUri = null)
        {
            // Case #1: Tha Case URI was provided
            // Case #2: The Case URI isn't provided so, it needs to taken from a different place
            if ((caseUri ??= queryBase.Notification.MainObjectUri).IsNotCase())
            {
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotCaseUri);
            }

            return await queryBase.ProcessGetAsync<Case>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseUri,  // Request URL
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCase);
        }

        /// <summary>
        /// Gets the <see cref="CaseStatuses"/> of the specific <see cref="Case"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="caseUri">The reference to the <see cref="Case"/> in <seealso cref="Uri"/> format.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal sealed async Task<CaseStatuses> TryGetCaseStatusesAsync(IQueryBase queryBase, Uri? caseUri)
        {
            // Case #1: The Case URI was provided
            // Case #2: The Case URI needs to obtained from elsewhere
            if ((caseUri ??= queryBase.Notification.MainObjectUri).IsNotCase())
            {
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotCaseUri);
            }

            // Predefined URL components
            string statusesEndpoint = $"https://{GetDomain()}/statussen";
            
            // Request URL
            Uri caseStatusesUri = new($"{statusesEndpoint}?zaak={caseUri}");

            return await queryBase.ProcessGetAsync<CaseStatuses>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: caseStatusesUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseStatuses);
        }

        /// <summary>
        /// Gets the most recent <see cref="CaseType"/> from <see cref="CaseStatuses"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="caseStatuses"><inheritdoc cref="CaseStatuses" path="/summary"/></param>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal sealed async Task<CaseType> GetLastCaseTypeAsync(IQueryBase queryBase, CaseStatuses caseStatuses)
        {
            // Request URL
            Uri lastStatusTypeUri = caseStatuses.LastStatus().TypeUri;

            return await queryBase.ProcessGetAsync<CaseType>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: lastStatusTypeUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCaseStatusType);
        }
        #pragma warning restore CA1822
        #endregion

        #region Abstract (BSN Number)
        /// <summary>
        /// Gets BSN number of a specific citizen linked to the <see cref="Case"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="caseUri">The <see cref="Case"/> in <see cref="Uri"/> format.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal async Task<string> GetBsnNumberAsync(IQueryBase queryBase, Uri caseUri)
        {
            // The provided URI is invalid
            if (caseUri.IsNotCase())
            {
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotCaseUri);
            }

            return (await GetCaseRoleAsync(queryBase, caseUri))
                .Party?
                .BsnNumber ?? string.Empty;
        }
        #endregion

        #region Abstract (Case Role)
        /// <summary>
        /// Gets the <see cref="CaseRole"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="caseUri">The <see cref="Case"/> in <see cref="Uri"/> format.</param>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal Task<CaseRole> GetCaseRoleAsync(IQueryBase queryBase, Uri caseUri);
        #endregion

        #region Abstract (Case type URI)
        /// <summary>
        /// Gets the <see cref="Case"/> type in <see cref="Uri"/> format from version-specific CaseDetails from "OpenZaak" Web API service.
        /// </summary>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="caseUri">The reference to the <see cref="Case"/> in <seealso cref="Uri"/> format.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal async Task<Uri> TryGetCaseTypeUriAsync(IQueryBase queryBase, Uri? caseUri = null)
        {
            // Case #1: The required URI is not provided so, it needs to be obtained from elsewhere
            if (caseUri == null)
            {
                // The Case type URI might be present in the initial notification; it can be returned
                if (queryBase.Notification.Attributes.CaseTypeUri.IsNotNullOrDefault())
                {
                    return queryBase.Notification.Attributes.CaseTypeUri;
                }

                // There is no obvious place from where the desired URI can be obtained
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotCaseTypeUri);
            }

            // Case #2: The provided URI is invalid
            if (caseUri.IsNotCase())
            {
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotCaseUri);
            }

            // Case #3: The Case type URI needs to be queried from Case URI
            return await GetCaseTypeUriAsync(queryBase, caseUri);
        }

        protected Task<Uri> GetCaseTypeUriAsync(IQueryBase queryBase, Uri caseUri);
        #endregion

        #region Polymorphic (Domain)
        /// <inheritdoc cref="IDomain.GetDomain"/>
        string IDomain.GetDomain() => this.Configuration.ZGW.Endpoint.OpenZaak();
        #endregion
    }
}