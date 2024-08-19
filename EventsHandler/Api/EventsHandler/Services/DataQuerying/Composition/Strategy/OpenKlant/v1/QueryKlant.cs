// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenKlant.Converters;
using EventsHandler.Mapping.Models.POCOs.OpenKlant.v1;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataSending.Clients.Enums;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Versioning.Interfaces;
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
        /// <inheritdoc cref="IQueryKlant.TryGetPartyDataAsync(IQueryBase, string, string)"/>
        async Task<CommonPartyData> IQueryKlant.TryGetPartyDataAsync(IQueryBase queryBase, string openKlantDomain, string bsnNumber)
        {
            // TODO: BSN number validation

            // Predefined URL components
            string citizensEndpoint = $"https://{openKlantDomain}/klanten/api/v1/klanten";

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
                uri: citizenByBsnUri,  // Request URL
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCitizenDetails);
        }
        #endregion

        #region Polymorphic (Telemetry)
        /// <inheritdoc cref="IQueryKlant.SendFeedbackAsync(IQueryBase, string, string)"/>
        async Task<ContactMoment> IQueryKlant.SendFeedbackAsync(IQueryBase queryBase, string openKlantDomain, string jsonBody)
        {
            // Predefined URL components
            var klantContactMomentUri = new Uri($"https://{openKlantDomain}/contactmomenten/api/v1/contactmomenten");

            // Sending the request and getting the response (combined internal logic)
            return await queryBase.ProcessPostAsync<ContactMoment>(
                httpClientType: HttpClientTypes.Telemetry_Contactmomenten,
                uri: klantContactMomentUri,  // Request URL
                jsonBody,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoFeedbackKlant);
        }
        #endregion
    }
}