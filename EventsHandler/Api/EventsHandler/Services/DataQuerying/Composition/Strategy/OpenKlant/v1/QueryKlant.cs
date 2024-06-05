// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.Converters;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v1;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.v1
{
    /// <inheritdoc cref="IQueryKlant"/>
    /// <remarks>
    ///   Version: "OpenKlant" (1.0) Web service | "OMC workflow" v1.
    /// </remarks>
    internal sealed class QueryKlant : IQueryKlant
    {
        /// <inheritdoc cref="IQueryKlant.Configuration"/>
        WebApiConfiguration IQueryKlant.Configuration { get; set; } = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryKlant"/> class.
        /// </summary>
        public QueryKlant(WebApiConfiguration configuration)
        {
            ((IQueryKlant)this).Configuration = configuration;
        }
        
        #region Polymorphic (Citizen details)
        /// <inheritdoc cref="IQueryKlant.GetPartyDataAsync(IQueryBase, string)"/>
        async Task<CommonPartyData> IQueryKlant.GetPartyDataAsync(
            IQueryBase queryBase, WebApiConfiguration configuration, string bsnNumber)
        {
            // Predefined URL components
            string citizensEndpoint = $"https://{configuration.User.Domain.OpenKlant()}/klanten/api/v1/klanten";
            
            // Request URL
            var citizenByBsnUri = new Uri($"{citizensEndpoint}?subjectNatuurlijkPersoon__inpBsn={bsnNumber}");

            return (await GetCitizenResultsV1Async(queryBase, citizenByBsnUri))
                .Citizen
                .ConvertToUnified();
        }

        private static async Task<CitizenResults> GetCitizenResultsV1Async(IQueryBase queryBase, Uri citizenByBsnUri)
        {
            return await queryBase.ProcessGetAsync<CitizenResults>(
                httpsClientType: HttpClientTypes.OpenKlant_v1,
                uri: citizenByBsnUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCitizenDetails);
        }
        #endregion
    }
}