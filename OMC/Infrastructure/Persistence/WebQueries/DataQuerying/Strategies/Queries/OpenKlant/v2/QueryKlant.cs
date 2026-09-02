// © 2024, Worth Systems.

using Common.Extensions;
using Common.Settings.Configuration;
using System.Text.Json;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataQuerying.Strategies.Queries.OpenKlant.Interfaces;
using WebQueries.DataSending.Clients.Enums;
using WebQueries.DataSending.Interfaces;
using WebQueries.Properties;
using WebQueries.Versioning.Interfaces;
using ZgwModels.Extensions;
using ZgwModels.Mapping.Models.POCOs.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.OpenKlant.Converters;
using ZgwModels.Mapping.Models.POCOs.OpenKlant.v2;
using ZgwModels.Properties;

namespace WebQueries.DataQuerying.Strategies.Queries.OpenKlant.v2
{
    /// <inheritdoc cref="IQueryKlant"/>
    /// <remarks>
    ///   Version: "OpenKlant" (v2) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    public sealed class QueryKlant : IQueryKlant
    {
        /// <inheritdoc cref="IQueryKlant.Configuration"/>
        OmcConfiguration IQueryKlant.Configuration { get; set; } = null!;

        /// <inheritdoc cref="IVersionDetails.Version"/>
        string IVersionDetails.Version => "2.0.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryKlant"/> class.
        /// </summary>
        public QueryKlant(OmcConfiguration configuration)  // Dependency Injection (DI)
        {
            ((IQueryKlant)this).Configuration = configuration;
        }

        #region Polymorphic (Party data)
        /// <inheritdoc cref="IQueryKlant.TryGetPartyDataAsync(IQueryBase, string, string?, bool, bool)"/>
        async Task<CommonPartyData> IQueryKlant.TryGetPartyDataAsync(IQueryBase queryBase, string bsnNumber, string? caseIdentifier, bool requireDigitalAddress, bool createIfMissing)
        {
            if (string.IsNullOrEmpty(bsnNumber))
            {
                throw new ArgumentException(QueryResources.Querying_ERROR_MissingBsnNumber_FromInitiatorRole);
            }

            // Predefined URL components
            string partiesEndpoint = $"{((IQueryKlant)this).Configuration.ZGW.Endpoint.OpenKlant()}/partijen";

            string partyIdentifier = ((IQueryKlant)this).Configuration.AppSettings.Variables.PartyIdentifier();
            string partyCodeTypeParameter = $"?partijIdentificator__codeSoortObjectId={partyIdentifier}";
            string partyObjectIdParameter = $"&partijIdentificator__objectId={bsnNumber}";
            const string expandParameter = "&expand=digitaleAdressen";

            // Request URL
            Uri partiesByTypeAndIdWithExpand = new($"{partiesEndpoint}{partyCodeTypeParameter}{partyObjectIdParameter}{expandParameter}");

            PartyResults results = await GetPartyResultsV2Async(queryBase, partiesByTypeAndIdWithExpand);  // Many party results

            if (createIfMissing && results.Results.IsEmpty())
            {
                // The party doesn't exist yet in OpenKlant - OMC's business flow doesn't guarantee one was
                // ever created for this citizen before this lookup (e.g. a print request can be the first
                // contact OMC ever has with them). Create a bare party carrying only the BSN identifier,
                // then re-run the exact same lookup: the fresh party has no digital addresses, so it falls
                // through PartyResults.Party's existing "no digital address on file" fallback unchanged.
                await CreatePartyAsync(queryBase, ((IQueryKlant)this).Configuration, partiesEndpoint, partyIdentifier, bsnNumber);

                results = await GetPartyResultsV2Async(queryBase, partiesByTypeAndIdWithExpand);
            }

            return results
                .Party(((IQueryKlant)this).Configuration,
                    caseIdentifier, requireDigitalAddress)  // Single determined party result
                .ConvertToUnified();
        }

        /// <summary>
        /// Creates a new "persoon" party in "OpenKlant" Web API service, carrying only the BSN identifier
        /// needed to make it findable by the lookup that triggered its creation - no name, address, or
        /// digital address is set, since none of that is available to OMC at this point.
        /// </summary>
        private static async Task CreatePartyAsync(IQueryBase queryBase, OmcConfiguration configuration, string partiesEndpoint, string codeSoortObjectId, string bsnNumber)
        {
            string codeObjectType = configuration.AppSettings.Variables.OpenKlant.CodeObjectType_Partij();
            string codeRegister = configuration.AppSettings.Variables.OpenKlant.CodeRegister_Partij();

            string jsonBody = GetCreatePartyJsonBody(codeObjectType, codeSoortObjectId, codeRegister, bsnNumber);

            await queryBase.ProcessPostAsync<PartyCreationResult>(
                httpClientType: HttpClientTypes.OpenKlant_v2,
                uri: new Uri(partiesEndpoint),  // Request URL
                jsonBody,
                fallbackErrorMessage: ZgwResources.HttpRequest_ERROR_NoPartyCreated);
        }

        /// <summary>
        /// Builds the JSON body creating a "persoon" party with a single BSN <c>partijIdentificator</c>,
        /// atomically, in one request - confirmed against the live "OpenKlant" instance to both create the
        /// party and make it immediately findable by the same query that triggered its creation.
        /// </summary>
        private static string GetCreatePartyJsonBody(string codeObjectType, string codeSoortObjectId, string codeRegister, string bsnNumber)
        {
            string safeCodeObjectType = JsonSerializer.Serialize(codeObjectType);
            string safeCodeSoortObjectId = JsonSerializer.Serialize(codeSoortObjectId);
            string safeCodeRegister = JsonSerializer.Serialize(codeRegister);
            string safeBsnNumber = JsonSerializer.Serialize(bsnNumber);

            return $"{{\"soortPartij\":\"persoon\",\"indicatieActief\":true," +
                   $"\"partijIdentificatoren\":[{{\"partijIdentificator\":{{" +
                   $"\"codeObjecttype\":{safeCodeObjectType}," +
                   $"\"codeSoortObjectId\":{safeCodeSoortObjectId}," +
                   $"\"codeRegister\":{safeCodeRegister}," +
                   $"\"objectId\":{safeBsnNumber}" +
                   $"}}}}]}}";
        }

        /// <inheritdoc cref="IQueryKlant.TryGetPartyDataAsync(IQueryBase, Uri, string?)"/>
        async Task<CommonPartyData> IQueryKlant.TryGetPartyDataAsync(IQueryBase queryBase, Uri involvedPartyUri, string? caseIdentifier)  // NOTE: This URI is the same as partijen from above
        {
            // The provided URI is invalid
            if (involvedPartyUri.IsNotParty())
            {
                throw new ArgumentException(QueryResources.Querying_ERROR_Internal_NotPartyUri);
            }

            // Predefined URL components
            const string expandParameter = "?expand=digitaleAdressen";

            // Request URL
            Uri partiesWithExpand = new($"{involvedPartyUri}{expandParameter}");

            return PartyResults.Party(  // Single determined party result
                    partyResult: await GetPartyResultV2Async(queryBase, partiesWithExpand),
                    configuration: ((IQueryKlant)this).Configuration,
                    caseIdentifier: caseIdentifier)
                .ConvertToUnified();
        }

        // NOTE: Multiple results
        private static async Task<PartyResults> GetPartyResultsV2Async(IQueryBase queryBase, Uri citizenUri)
        {
            return await queryBase.ProcessGetAsync<PartyResults>(
                httpClientType: HttpClientTypes.OpenKlant_v2,
                uri: citizenUri,  // Request URL
                fallbackErrorMessage: ZgwResources.HttpRequest_ERROR_NoPartyResults);
        }

        // NOTE: Single result
        private static async Task<PartyResult> GetPartyResultV2Async(IQueryBase queryBase, Uri citizenUri)
        {
            return await queryBase.ProcessGetAsync<PartyResult>(
                httpClientType: HttpClientTypes.OpenKlant_v2,
                uri: citizenUri,  // Request URL
                fallbackErrorMessage: ZgwResources.HttpRequest_ERROR_NoPartyResults);
        }
        #endregion

        #region Polymorphic (Telemetry)
        /// <inheritdoc cref="IQueryKlant.CreateNewContactMomentAsync(IQueryBase, string)"/>
        async Task<MaakKlantContact> IQueryKlant.CreateNewContactMomentAsync(IQueryBase queryBase, string jsonBody)
        {
            // Predefined URL components
            Uri klantContactMomentUri = new($"{((IQueryKlant)this).Configuration.ZGW.Endpoint.OpenKlant()}/maak-klantcontact");

            // Sending the request
            return await queryBase.ProcessPostAsync<MaakKlantContact>(
                httpClientType: HttpClientTypes.Telemetry_Klantinteracties,
                uri: klantContactMomentUri,  // Request URL
                jsonBody,
                fallbackErrorMessage: ZgwResources.HttpRequest_ERROR_NoFeedbackKlant);
        }

        /// <inheritdoc cref="IQueryKlant.CreateBijlageAsync(IHttpNetworkService, string)"/>
        async Task<HttpRequestResponse> IQueryKlant.CreateBijlageAsync(IHttpNetworkService networkService, string jsonBody)
        {
            Uri bijlageUri = new($"{((IQueryKlant)this).Configuration.ZGW.Endpoint.OpenKlant()}/bijlagen");

            // Sending the request
            return await networkService.PostAsync(
                httpClientType: HttpClientTypes.Telemetry_Klantinteracties,
                uri: bijlageUri,  // Request URL
                jsonBody);
        }

        /// <inheritdoc cref="IQueryKlant.CreateContactMomentAsync(IQueryBase, string)"/>
        async Task<ContactMoment> IQueryKlant.CreateContactMomentAsync(IQueryBase queryBase, string jsonBody)
        {
            // Predefined URL components
            Uri klantContactMomentUri = new($"{((IQueryKlant)this).Configuration.ZGW.Endpoint.OpenKlant()}/klantcontacten");

            // Sending the request
            return await queryBase.ProcessPostAsync<ContactMoment>(
                httpClientType: HttpClientTypes.Telemetry_Klantinteracties,
                uri: klantContactMomentUri,  // Request URL
                jsonBody,
                fallbackErrorMessage: ZgwResources.HttpRequest_ERROR_NoFeedbackKlant);
        }

        /// <inheritdoc cref="IQueryKlant.LinkCaseToContactMomentAsync(IHttpNetworkService, string)"/>
        async Task<HttpRequestResponse> IQueryKlant.LinkCaseToContactMomentAsync(IHttpNetworkService networkService, string jsonBody)
        {
            // Predefined URL components
            Uri objectContactMomentUri = new($"{((IQueryKlant)this).Configuration.ZGW.Endpoint.OpenKlant()}/onderwerpobjecten");

            // Sending the request
            return await networkService.PostAsync(
                httpClientType: HttpClientTypes.Telemetry_Klantinteracties,
                uri: objectContactMomentUri,  // Request URL
                jsonBody);
        }

        /// <inheritdoc cref="IQueryKlant.LinkPartyToContactMomentAsync"/>
        async Task<HttpRequestResponse> IQueryKlant.LinkPartyToContactMomentAsync(IHttpNetworkService networkService, string jsonBody)
        {
            // Predefined URL components
            Uri customerContactMomentUri = new($"{((IQueryKlant)this).Configuration.ZGW.Endpoint.OpenKlant()}/betrokkenen");

            // Sending the request
            return await networkService.PostAsync(
                httpClientType: HttpClientTypes.Telemetry_Klantinteracties,
                uri: customerContactMomentUri,  // Request URL
                jsonBody);
        }

        /// <inheritdoc cref="IQueryKlant.LinkActorToContactMomentAsync"/>
        async Task<HttpRequestResponse> IQueryKlant.LinkActorToContactMomentAsync(IHttpNetworkService networkService, string jsonBody)
        {
            Uri customerContactMomentUri = new($"{((IQueryKlant)this).Configuration.ZGW.Endpoint.ContactMomenten()}/actorklantcontacten");

            // Sending the request
            return await networkService.PostAsync(
                httpClientType: HttpClientTypes.Telemetry_Klantinteracties,
                uri: customerContactMomentUri,  // Request URL
                jsonBody);
        }
        #endregion

        #region Polymorphic (Health Check)
        /// <inheritdoc cref="IDomain.GetHealthCheckAsync(IHttpNetworkService)"/>
        async Task<HttpRequestResponse> IDomain.GetHealthCheckAsync(IHttpNetworkService networkService)
        {
            Uri healthCheckEndpointUri = new($"{((IQueryKlant)this).GetDomain()}/klantcontacten");  // NOTE: There is no dedicated health check endpoint, calling anything should be fine

            return await networkService.GetAsync(HttpClientTypes.OpenKlant_v2, healthCheckEndpointUri);
        }
        #endregion
    }
}