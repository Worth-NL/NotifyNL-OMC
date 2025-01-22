// © 2024, Worth Systems.

using Common.Settings.Configuration;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenKlant.Interfaces;
using WebQueries.DataSending.Clients.Enums;
using WebQueries.DataSending.Interfaces;
using WebQueries.Properties;
using WebQueries.Versioning.Interfaces;
using ZhvModels.Extensions;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;
using ZhvModels.Mapping.Models.POCOs.OpenKlant.Converters;
using ZhvModels.Mapping.Models.POCOs.OpenKlant.v1;
using ZhvModels.Properties;

namespace WebQueries.DataQuerying.Strategies.Queries.OpenKlant.v1
{
    /// <inheritdoc cref="IQueryKlant"/>
    /// <remarks>
    ///   Version: "OpenKlant" (v1) Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    public sealed class QueryKlant : IQueryKlant
    {
        /// <inheritdoc cref="IQueryKlant.Configuration"/>
        OmcConfiguration IQueryKlant.Configuration { get; set; } = null!;

        /// <inheritdoc cref="IVersionDetails.Version"/>
        string IVersionDetails.Version => "1.0.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryKlant"/> class.
        /// </summary>
        public QueryKlant(OmcConfiguration configuration)  // Dependency Injection (DI)
        {
            ((IQueryKlant)this).Configuration = configuration;
        }

        #region Polymorphic (Party data)
        /// <inheritdoc cref="IQueryKlant.TryGetPartyDataAsync(IQueryBase, string)"/>
        async Task<CommonPartyData> IQueryKlant.TryGetPartyDataAsync(IQueryBase queryBase, string bsnNumber)
        {
            // Predefined URL components
            string citizensEndpoint = $"https://{((IQueryKlant)this).Configuration.ZGW.Endpoint.OpenKlant()}/klanten";

            // Request URL
            Uri citizenByBsnUri = new($"{citizensEndpoint}?subjectNatuurlijkPersoon__inpBsn={bsnNumber}");

            return (await GetPartyResultsV1Async(queryBase, citizenByBsnUri))  // Many party results
                .Party  // Single determined party result
                .ConvertToUnified();
        }

        /// <inheritdoc cref="IQueryKlant.TryGetPartyDataAsync(IQueryBase, Uri)"/>
        async Task<CommonPartyData> IQueryKlant.TryGetPartyDataAsync(IQueryBase queryBase, Uri involvedPartyUri)
        {
            // The provided URI is invalid
            if (involvedPartyUri.IsNotParty())
            {
                throw new ArgumentException(QueryResources.Querying_ERROR_Internal_NotPartyUri);
            }

            // Single determined party result
            return (await GetPartyResultV1Async(queryBase, involvedPartyUri))  // Request URL
                .ConvertToUnified();
        }

        /// <inheritdoc cref="IQueryKlant.TryGetPartyDataAsync(IQueryBase, Uri)"/>
        async Task<CommonPartyData> IQueryKlant.TryGetPartyDataAsync(IQueryBase queryBase, Uri involvedPartyUri, string? caseIdentifier)
        {
            // The provided URI is invalid
            if (involvedPartyUri.IsNotParty())
            {
                throw new ArgumentException(QueryResources.Querying_ERROR_Internal_NotPartyUri);
            }

            // Single determined party result
            return (await GetPartyResultV1Async(queryBase, involvedPartyUri))  // Request URL
                .ConvertToUnified();
        }

        // NOTE: Multiple results
        private static async Task<PartyResults> GetPartyResultsV1Async(IQueryBase queryBase, Uri citizenUri)
        {
            return await queryBase.ProcessGetAsync<PartyResults>(
                httpClientType: HttpClientTypes.OpenKlant_v1,
                uri: citizenUri,  // Request URL
                fallbackErrorMessage: ZhvResources.HttpRequest_ERROR_NoPartyResults);
        }

        // NOTE: Single result
        private static async Task<PartyResult> GetPartyResultV1Async(IQueryBase queryBase, Uri citizenUri)
        {
            return await queryBase.ProcessGetAsync<PartyResult>(
                httpClientType: HttpClientTypes.OpenKlant_v1,
                uri: citizenUri,  // Request URL
                fallbackErrorMessage: ZhvResources.HttpRequest_ERROR_NoPartyResults);
        }
        #endregion

        #region Polymorphic (Telemetry)
        /// <inheritdoc cref="IQueryKlant.CreateContactMomentAsync(IQueryBase, string)"/>
        async Task<ContactMoment> IQueryKlant.CreateContactMomentAsync(IQueryBase queryBase, string jsonBody)
        {
            // Predefined URL components
            Uri klantContactMomentUri = new($"https://{((IQueryKlant)this).Configuration.ZGW.Endpoint.ContactMomenten()}/contactmomenten");

            // Sending the request
            return await queryBase.ProcessPostAsync<ContactMoment>(
                httpClientType: HttpClientTypes.Telemetry_Contactmomenten,
                uri: klantContactMomentUri,  // Request URL
                jsonBody,
                fallbackErrorMessage: ZhvResources.HttpRequest_ERROR_NoFeedbackKlant);
        }

        /// <inheritdoc cref="IQueryKlant.LinkCaseToContactMomentAsync(IHttpNetworkService, string)"/>
        async Task<HttpRequestResponse> IQueryKlant.LinkCaseToContactMomentAsync(IHttpNetworkService networkService, string jsonBody)
        {
            // Predefined URL components
            Uri objectContactMomentUri = new($"https://{((IQueryKlant)this).Configuration.ZGW.Endpoint.OpenZaak()}/zaakcontactmomenten");

            // Sending the request
            return await networkService.PostAsync(
                httpClientType: HttpClientTypes.Telemetry_Contactmomenten,
                uri: objectContactMomentUri,  // Request URL
                jsonBody);
        }

        /// <inheritdoc cref="IQueryKlant.LinkPartyToContactMomentAsync"/>
        async Task<HttpRequestResponse> IQueryKlant.LinkPartyToContactMomentAsync(IHttpNetworkService networkService, string jsonBody)
        {
            // Predefined URL components
            Uri customerContactMomentUri = new($"https://{((IQueryKlant)this).Configuration.ZGW.Endpoint.ContactMomenten()}/klantcontactmomenten");

            // Sending the request
            return await networkService.PostAsync(
                httpClientType: HttpClientTypes.Telemetry_Contactmomenten,
                uri: customerContactMomentUri,  // Request URL
                jsonBody);
        }
        #endregion

        #region Polymorphic (Health Check)
        /// <inheritdoc cref="IDomain.GetHealthCheckAsync(IHttpNetworkService)"/>
        async Task<HttpRequestResponse> IDomain.GetHealthCheckAsync(IHttpNetworkService networkService)
        {
            Uri healthCheckEndpointUri = new($"https://{((IQueryKlant)this).GetDomain()}/klanten");  // NOTE: There is no dedicated health check endpoint, calling anything should be fine

            return await networkService.GetAsync(HttpClientTypes.OpenKlant_v1, healthCheckEndpointUri);
        }
        #endregion
    }
}