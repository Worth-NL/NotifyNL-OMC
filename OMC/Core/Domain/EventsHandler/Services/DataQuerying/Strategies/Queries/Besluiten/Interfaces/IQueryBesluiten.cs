// © 2024, Worth Systems.

using Common.Settings.Configuration;
using EventsHandler.Properties;
using EventsHandler.Services.DataQuerying.Strategies.Interfaces;
using EventsHandler.Services.DataSending.Clients.Enums;
using EventsHandler.Services.Versioning.Interfaces;
using System.Text.Json;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision;
using ZhvModels.Properties;

namespace EventsHandler.Services.DataQuerying.Strategies.Queries.Besluiten.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    /// <seealso cref="IDomain"/>
    public interface IQueryBesluiten : IVersionDetails, IDomain
    {
        /// <inheritdoc cref="WebApiConfiguration"/>
        protected internal WebApiConfiguration Configuration { get; set; }

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "Besluiten";

        #region Parent (DecisionResource)
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
                throw new ArgumentException(ApiResources.Operation_ERROR_Internal_NotDecisionResourceUri);
            }

            return await queryBase.ProcessGetAsync<DecisionResource>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: resourceUri,  // Request URL
                fallbackErrorMessage: ZhvResources.HttpRequest_ERROR_NoDecisionResource);
        }
        #endregion

        #region Parent (InfoObject)
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
                throw new ArgumentException(ApiResources.Operation_ERROR_Internal_NotInfoObjectUri);
            }

            return await queryBase.ProcessGetAsync<InfoObject>(
                httpClientType: HttpClientTypes.OpenZaak_v1,
                uri: infoObjectUri,
                fallbackErrorMessage: ZhvResources.HttpRequest_ERROR_NoInfoObject);
        }
        #endregion

        #region Parent (Decision)
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
                fallbackErrorMessage: ZhvResources.HttpRequest_ERROR_NoDecision);
        }
        #endregion

        #region Parent (Document)
        /// <summary>
        /// Gets the <see cref="Documents"/> from "OpenZaak" Web API service.
        /// </summary>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="decisionResource"><inheritdoc cref="DecisionResource" path="/summary"/></param>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal async Task<Documents> TryGetDocumentsAsync(IQueryBase queryBase, DecisionResource? decisionResource)
        {
            // Predefined request URL components
            string documentsEndpoint = $"https://{GetDomain()}/besluitinformatieobjecten";

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
                fallbackErrorMessage: ZhvResources.HttpRequest_ERROR_NoDocuments);
        }
        #endregion

        #region Parent (DecisionType)
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
                fallbackErrorMessage: ZhvResources.HttpRequest_ERROR_NoDecisionType);
        }
        #endregion)

        #region Polymorphic (Domain)
        /// <inheritdoc cref="IDomain.GetDomain"/>
        string IDomain.GetDomain() => Configuration.ZGW.Endpoint.Besluiten();
        #endregion
    }
}