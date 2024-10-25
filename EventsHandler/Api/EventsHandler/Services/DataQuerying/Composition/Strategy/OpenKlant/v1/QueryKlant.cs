// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenKlant.Converters;
using EventsHandler.Mapping.Models.POCOs.OpenKlant.v1;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataSending.Clients.Enums;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.DataSending.Responses;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Versioning.Interfaces;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.v1
{
    /// <inheritdoc cref="IQueryKlant"/>
    /// <remarks>
    ///   Version: "OpenKlant" (v1) Web API service | "OMC workflow" v1.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    internal sealed class QueryKlant : IQueryKlant
    {
        /// <inheritdoc cref="IQueryKlant.Configuration"/>
        WebApiConfiguration IQueryKlant.Configuration { get; set; } = null!;

        /// <inheritdoc cref="IVersionDetails.Version"/>
        string IVersionDetails.Version => "1.0.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryKlant"/> class.
        /// </summary>
        public QueryKlant(WebApiConfiguration configuration)
        {
            ((IQueryKlant)this).Configuration = configuration;
        }

        #region Polymorphic (Party data)
        /// <inheritdoc cref="IQueryKlant.TryGetPartyDataAsync(IQueryBase, string)"/>
        async Task<CommonPartyData> IQueryKlant.TryGetPartyDataAsync(IQueryBase queryBase, string bsnNumber)
        {
            // Predefined URL components
            string citizensEndpoint = $"https://{((IQueryKlant)this).Configuration.ZGW.Domain.OpenKlant()}/klanten";

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
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotPartyUri);
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
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoPartyResults);
        }

        // NOTE: Single result
        private static async Task<PartyResult> GetPartyResultV1Async(IQueryBase queryBase, Uri citizenUri)
        {
            return await queryBase.ProcessGetAsync<PartyResult>(
                httpClientType: HttpClientTypes.OpenKlant_v1,
                uri: citizenUri,  // Request URL
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoPartyResults);
        }
        #endregion

        #region Polymorphic (Telemetry)
        /// <inheritdoc cref="IQueryKlant.CreateContactMomentAsync(IQueryBase, string)"/>
        async Task<ContactMoment> IQueryKlant.CreateContactMomentAsync(IQueryBase queryBase, string jsonBody)
        {
            // Predefined URL components
            Uri klantContactMomentUri = new($"https://{((IQueryKlant)this).Configuration.ZGW.Domain.ContactMomenten()}/contactmomenten");

            // Sending the request
            return await queryBase.ProcessPostAsync<ContactMoment>(
                httpClientType: HttpClientTypes.Telemetry_Contactmomenten,
                uri: klantContactMomentUri,  // Request URL
                jsonBody,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoFeedbackKlant);
        }

        /// <inheritdoc cref="IQueryKlant.LinkCaseToContactMomentAsync(IHttpNetworkService, string)"/>
        async Task<RequestResponse> IQueryKlant.LinkCaseToContactMomentAsync(IHttpNetworkService networkService, string jsonBody)
        {
            // Predefined URL components
            Uri objectContactMomentUri = new($"https://{((IQueryKlant)this).Configuration.ZGW.Domain.OpenZaak()}/zaakcontactmomenten");
            
            // Sending the request
            return await networkService.PostAsync(
                httpClientType: HttpClientTypes.Telemetry_Contactmomenten,
                uri: objectContactMomentUri,  // Request URL
                jsonBody);
        }

        /// <inheritdoc cref="IQueryKlant.LinkCustomerToContactMomentAsync(IHttpNetworkService, string)"/>
        async Task<RequestResponse> IQueryKlant.LinkCustomerToContactMomentAsync(IHttpNetworkService networkService, string jsonBody)
        {
            // Predefined URL components
            Uri customerContactMomentUri = new($"https://{((IQueryKlant)this).Configuration.ZGW.Domain.ContactMomenten()}/klantcontactmomenten");

            // Sending the request
            return await networkService.PostAsync(
                httpClientType: HttpClientTypes.Telemetry_Contactmomenten,
                uri: customerContactMomentUri,  // Request URL
                jsonBody);
        }
        #endregion
    }
}