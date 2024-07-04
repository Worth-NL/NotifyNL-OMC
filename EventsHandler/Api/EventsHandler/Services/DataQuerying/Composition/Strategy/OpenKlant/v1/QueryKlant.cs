// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.Converters;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v1;
using EventsHandler.Behaviors.Versioning;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.v1
{
    /// <inheritdoc cref="IQueryKlant"/>
    /// <remarks>
    ///   Version: "OpenKlant" (v1+) Web API service | "OMC workflow" v1.
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
        
        #region Polymorphic (Citizen details)
        /// <inheritdoc cref="IQueryKlant.GetPartyDataAsync(IQueryBase, string)"/>
        async Task<CommonPartyData> IQueryKlant.GetPartyDataAsync(IQueryBase queryBase, string bsnNumber)
        {
            // Predefined URL components
            string citizensEndpoint = $"https://{((IQueryKlant)this).GetSpecificOpenKlantDomain()}/klanten/api/v1/klanten";
            
            // Request URL
            var citizenByBsnUri = new Uri($"{citizensEndpoint}?subjectNatuurlijkPersoon__inpBsn={bsnNumber}");

            return (await GetCitizenResultsV1Async(queryBase, citizenByBsnUri))
                .Citizen
                .ConvertToUnified();
        }

        private static async Task<CitizenResults> GetCitizenResultsV1Async(IQueryBase queryBase, Uri citizenByBsnUri)
        {
            return await queryBase.ProcessGetAsync<CitizenResults>(
                httpClientType: HttpClientTypes.OpenKlant_v1,
                uri: citizenByBsnUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCitizenDetails);
        }
        #endregion

        #region Polymorphic (Telemetry)
        /// <inheritdoc cref="IQueryKlant.SendFeedbackAsync"/>
        async Task<ContactMoment> IQueryKlant.SendFeedbackAsync(IQueryBase queryBase, HttpContent body)
        {
            // Predefined URL components
            var klantContactMomentUri = new Uri($"https://{((IQueryKlant)this).GetSpecificOpenKlantDomain()}/contactmomenten/api/v1/contactmomenten");
            
            // Sending the request and getting the response (combined internal logic)
            return await queryBase.ProcessPostAsync<ContactMoment>(
                httpClientType: HttpClientTypes.Telemetry_Contactmomenten,
                uri: klantContactMomentUri,
                body: body,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoFeedbackKlant);
        }
        #endregion
    }
}