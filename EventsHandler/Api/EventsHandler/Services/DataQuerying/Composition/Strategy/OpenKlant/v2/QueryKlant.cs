﻿// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenKlant.Converters;
using EventsHandler.Mapping.Models.POCOs.OpenKlant.v2;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataSending.Clients.Enums;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.DataSending.Responses;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Versioning.Interfaces;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.v2
{
    /// <inheritdoc cref="IQueryKlant"/>
    /// <remarks>
    ///   Version: "OpenKlant" (v2+) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    internal sealed class QueryKlant : IQueryKlant
    {
        /// <inheritdoc cref="IQueryKlant.Configuration"/>
        WebApiConfiguration IQueryKlant.Configuration { get; set; } = null!;

        /// <inheritdoc cref="IVersionDetails.Version"/>
        string IVersionDetails.Version => "2.0.0";

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
            string partiesEndpoint = $"https://{((IQueryKlant)this).Configuration.User.Domain.OpenKlant()}/partijen";

            string partyIdentifier = ((IQueryKlant)this).Configuration.AppSettings.Variables.PartyIdentifier();
            string partyCodeTypeParameter = $"?partijIdentificator__codeSoortObjectId={partyIdentifier}";
            string partyObjectIdParameter = $"&partijIdentificator__objectId={bsnNumber}";
            const string expandParameter = "&expand=digitaleAdressen";

            // Request URL
            Uri partiesByTypeAndIdWithExpand = new($"{partiesEndpoint}{partyCodeTypeParameter}{partyObjectIdParameter}{expandParameter}");

            return (await GetPartyResultsV2Async(queryBase, partiesByTypeAndIdWithExpand))  // Many party results
                .Party(((IQueryKlant)this).Configuration)  // Single determined party result
                .ConvertToUnified();
        }

        /// <inheritdoc cref="IQueryKlant.TryGetPartyDataAsync(IQueryBase, Uri)"/>
        async Task<CommonPartyData> IQueryKlant.TryGetPartyDataAsync(IQueryBase queryBase, Uri involvedPartyUri)  // NOTE: This URI is the same as partijen from above
        {
            // The provided URI is invalid
            if (involvedPartyUri.IsNotParty())
            {
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotPartyUri);
            }

            // Predefined URL components
            const string expandParameter = "?expand=digitaleAdressen";

            // Request URL
            Uri partiesWithExpand = new($"{involvedPartyUri}{expandParameter}");

            return PartyResults.Party(  // Single determined party result
                    partyResult: await GetPartyResultV2Async(queryBase, partiesWithExpand),
                    configuration: ((IQueryKlant)this).Configuration)
                .ConvertToUnified();
        }

        // NOTE: Multiple results
        private static async Task<PartyResults> GetPartyResultsV2Async(IQueryBase queryBase, Uri citizenUri)
        {
            return await queryBase.ProcessGetAsync<PartyResults>(
                httpClientType: HttpClientTypes.OpenKlant_v2,
                uri: citizenUri,  // Request URL
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoPartyResults);
        }

        // NOTE: Single result
        private static async Task<PartyResult> GetPartyResultV2Async(IQueryBase queryBase, Uri citizenUri)
        {
            return await queryBase.ProcessGetAsync<PartyResult>(
                httpClientType: HttpClientTypes.OpenKlant_v2,
                uri: citizenUri,  // Request URL
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoPartyResults);
        }
        #endregion

        #region Polymorphic (Telemetry)
        /// <inheritdoc cref="IQueryKlant.CreateContactMomentAsync(IQueryBase, string)"/>
        async Task<ContactMoment> IQueryKlant.CreateContactMomentAsync(IQueryBase queryBase,string jsonBody)
        {
            // Predefined URL components
            Uri klantContactMomentUri = new($"https://{((IQueryKlant)this).Configuration.User.Domain.OpenKlant()}/klantcontacten");

            // Sending the request
            return await queryBase.ProcessPostAsync<ContactMoment>(
                httpClientType: HttpClientTypes.Telemetry_Klantinteracties,
                uri: klantContactMomentUri,  // Request URL
                jsonBody,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoFeedbackKlant);
        }

        /// <inheritdoc cref="IQueryKlant.LinkCaseToContactMomentAsync(IHttpNetworkService, string)"/>
        async Task<RequestResponse> IQueryKlant.LinkCaseToContactMomentAsync(IHttpNetworkService networkService, string jsonBody)
        {
            // Predefined URL components
            Uri objectContactMomentUri = new($"https://{((IQueryKlant)this).Configuration.User.Domain.OpenKlant()}/onderwerpobjecten");
            
            // Sending the request
            return await networkService.PostAsync(
                httpClientType: HttpClientTypes.Telemetry_Klantinteracties,
                uri: objectContactMomentUri,  // Request URL
                jsonBody);
        }

        /// <inheritdoc cref="IQueryKlant.LinkCustomerToContactMomentAsync(IHttpNetworkService, string)"/>
        async Task<RequestResponse> IQueryKlant.LinkCustomerToContactMomentAsync(IHttpNetworkService networkService, string jsonBody)
        {
            // Predefined URL components
            Uri customerContactMomentUri = new($"https://{((IQueryKlant)this).Configuration.User.Domain.OpenKlant()}/betrokkenen");

            // Sending the request
            return await networkService.PostAsync(
                httpClientType: HttpClientTypes.Telemetry_Klantinteracties,
                uri: customerContactMomentUri,  // Request URL
                jsonBody);
        }
        #endregion
    }
}