// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.Converters;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v2;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.v2
{
    /// <inheritdoc cref="IQueryKlant"/>
    /// <remarks>
    ///   Version: "OpenKlant" (2.0) Web service.
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
            string partiesEndpoint = $"https://{configuration.User.Domain.OpenKlant()}/klantinteracties/api/v1/partijen";
            
            string partyTypeParameter = $"?partijIdentificator__codeSoortObjectId={configuration.AppSettings.Variables.PartyIdentifier()}";
            string partyObjectIdParameter = $"&partijIdentificator__objectId={bsnNumber}";
            const string expandParameter = "&expand=digitaleAdressen";

            // Request URL
            var partiesByTypeIdAndExpand
                = new Uri($"{partiesEndpoint}{partyTypeParameter}{partyObjectIdParameter}{expandParameter}");

            return (await GetCitizenDetailsV2Async(queryBase, partiesByTypeIdAndExpand))
                .Party()
                .ConvertToUnified();
        }

        private static async Task<PartyResults> GetCitizenDetailsV2Async(IQueryBase queryBase, Uri citizenByBsnUri)
        {
            return await queryBase.ProcessGetAsync<PartyResults>(
                httpsClientType: HttpClientTypes.OpenKlant_v2,
                uri: citizenByBsnUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCitizenDetails);
        }
        #endregion
    }
}