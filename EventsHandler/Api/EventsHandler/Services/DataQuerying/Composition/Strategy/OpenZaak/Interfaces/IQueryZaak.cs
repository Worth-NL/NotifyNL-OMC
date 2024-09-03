// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Mapping.Models.POCOs.OpenZaak.Decision;
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
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal sealed async Task<CaseStatuses> TryGetCaseStatusesAsync(IQueryBase queryBase, Uri? caseUri)
        {
            // Predefined URL components
            string statusesEndpoint = $"https://{GetDomain()}/zaken/api/v1/statussen";
            
            // Case #1: The Case URI was provided
            // Case #2: The Case URI needs to obtained from elsewhere
            if ((caseUri ??= queryBase.Notification.MainObjectUri).IsNotCase())
            {
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotCaseUri);
            }

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

        #region Parent (Main Object)
        #pragma warning disable CA1822  // Method(s) can be marked as static but that would be inconsistent for interface
        /// <summary>
        /// Gets the <see cref="MainObject"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal sealed async Task<MainObject> GetMainObjectAsync(IQueryBase queryBase)
        {
            return await queryBase.ProcessGetAsync<MainObject>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: queryBase.Notification.MainObjectUri,  // Request URL
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoMainObject);
        }
        #pragma warning restore CA1822
        #endregion

        #region Parent (Decision)
        /// <summary>
        /// Gets the <see cref="DecisionResource"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="resourceUri">The resource <see cref="Uri"/>.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal async Task<DecisionResource> TryGetDecisionResourceAsync(IQueryBase queryBase, Uri? resourceUri)
        {
            // Case #1: The Resource URI was provided
            // Case #2: The Resource URI needs to obtained from elsewhere
            if ((resourceUri ??= queryBase.Notification.ResourceUri).IsNotDecisionResource())
            {
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotDecisionResourceUri);
            }

            return await queryBase.ProcessGetAsync<DecisionResource>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: resourceUri,  // Request URL
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoDecisionResource);
        }

        /// <summary>
        /// Gets the <see cref="InfoObject"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="parameter">
        ///   <list type="number">
        ///     <item>Nothing => Info Object <see cref="Uri"/> will be queried from 2. <see cref="DecisionResource"/></item>
        ///     <item><see cref="DecisionResource"/> => containing Info Object <see cref="Uri"/></item>
        ///     <item><see cref="Document"/> => containing Info Object <see cref="Uri"/></item>
        ///   </list>
        /// </param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal async Task<InfoObject> TryGetInfoObjectAsync(IQueryBase queryBase, object? parameter)
        {
            // Request URL
            Uri infoObjectUri;

            // Case #1: The InfoObject URI isn't provided so, it needs to be re-queried
            if (parameter == null)
            {
                infoObjectUri = (await TryGetDecisionResourceAsync(queryBase, queryBase.Notification.ResourceUri))
                                .InfoObjectUri;
            }
            // Case #2: The info object URI can be used directly from DecisionResource
            else if (parameter is DecisionResource decisionResource)
            {
                infoObjectUri = decisionResource.InfoObjectUri;
            }
            // Case #3: The InfoObject URI can be used directly from Document
            else if (parameter is Document document)
            {
                infoObjectUri = document.InfoObjectUri;
            }
            // Case #4: Unhandled situation occurred
            else
            {
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotInfoObjectUri);
            }

            return await queryBase.ProcessGetAsync<InfoObject>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: infoObjectUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoInfoObject);
        }

        /// <summary>
        /// Gets the <see cref="Decision"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="decisionResource"><inheritdoc cref="DecisionResource" path="/summary"/></param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal async Task<Decision> TryGetDecisionAsync(IQueryBase queryBase, DecisionResource? decisionResource)
        {
            // Request URL
            Uri decisionUri =
                // Case #1: The Decision URI can be extracted directly from DecisionResource
                // Case #2: Re-querying DecisionResource first to get the Decision URI later
                (decisionResource ?? await TryGetDecisionResourceAsync(queryBase, queryBase.Notification.ResourceUri))
                .DecisionUri;

            return await queryBase.ProcessGetAsync<Decision>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: decisionUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoDecision);
        }
        
        /// <summary>
        /// Gets the <see cref="Documents"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="decisionResource"><inheritdoc cref="DecisionResource" path="/summary"/></param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal async Task<Documents> TryGetDocumentsAsync(IQueryBase queryBase, DecisionResource? decisionResource)
        {
            // Predefined request URL components
            string documentsEndpoint = $"https://{GetDomain()}/besluiten/api/v1/besluitinformatieobjecten";

            Uri decisionUri =
                // Case #1: The Decision URI can be extracted directly from DecisionResource
                // Case #2: Re-querying DecisionResource first to get the Decision URI later
                (decisionResource ?? await TryGetDecisionResourceAsync(queryBase, queryBase.Notification.ResourceUri))
                .DecisionUri;

            // Request URL
            Uri documentsUri = new($"{documentsEndpoint}?besluit={decisionUri}");
            
            return await queryBase.ProcessGetAsync<Documents>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: documentsUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoDocuments);
        }
        
        /// <summary>
        /// Gets the <see cref="DecisionType"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="decision"><inheritdoc cref="Decision" path="/summary"/></param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal async Task<DecisionType> TryGetDecisionTypeAsync(IQueryBase queryBase, Decision? decision)
        {
            // Request URL
            Uri decisionTypeUri;

            // Case #1: The Decision is missing
            if (decision == null)
            {
                decisionTypeUri = queryBase.Notification.Attributes.DecisionTypeUri.IsNotNullOrDefault()
                    // Variant A: But can be obtained directly from the initial notification
                    ? queryBase.Notification.Attributes.DecisionTypeUri
                    // Variant B: And it can be attempted to be re-queried from "OpenZaak"
                    : (await TryGetDecisionAsync(queryBase, null)).TypeUri;
            }
            // Case #2: The required URI can be retrieved directly from the given Decision
            else
            {
                decisionTypeUri = decision.Value.TypeUri;
            }
            
            return await queryBase.ProcessGetAsync<DecisionType>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: decisionTypeUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoDecisionType);
        }
        #endregion

        #region Abstract (BSN Number)
        /// <summary>
        /// Gets BSN number of a specific citizen linked to the <see cref="Case"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="openZaakDomain">The domain of <see cref="IQueryZaak"/> Web API service.</param>
        /// <param name="caseUri">The <see cref="Case"/> in <see cref="Uri"/> format.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal async Task<string> GetBsnNumberAsync(IQueryBase queryBase, string openZaakDomain, Uri caseUri)
        {
            // The provided URI is invalid
            if (caseUri.IsNotCase())
            {
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotCaseUri);
            }

            return await PolymorphicGetBsnNumberAsync(queryBase, openZaakDomain, caseUri);
        }

        /// <inheritdoc cref="GetBsnNumberAsync(IQueryBase, string, Uri)"/>
        protected Task<string> PolymorphicGetBsnNumberAsync(IQueryBase queryBase, string openZaakDomain, Uri caseUri);
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
            return await PolymorphicGetCaseTypeUriAsync(queryBase, caseUri);
        }

        /// <inheritdoc cref="TryGetCaseTypeUriAsync(IQueryBase, Uri)"/>
        protected Task<Uri> PolymorphicGetCaseTypeUriAsync(IQueryBase queryBase, Uri caseUri);
        #endregion

        #region Polymorphic (Domain)
        /// <inheritdoc cref="IDomain.GetDomain"/>
        string IDomain.GetDomain() => this.Configuration.User.Domain.OpenZaak();
        #endregion
    }
}