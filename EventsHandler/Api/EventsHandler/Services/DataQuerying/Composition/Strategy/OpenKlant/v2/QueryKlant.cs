// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.Converters;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v2;
using EventsHandler.Exceptions;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataSending.Clients.Enums;
using EventsHandler.Services.DataSending.Interfaces;
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

        #region Polymorphic (Citizen details)
        /// <inheritdoc cref="IQueryKlant.GetPartyDataAsync(IQueryBase, string)"/>
        async Task<CommonPartyData> IQueryKlant.GetPartyDataAsync(IQueryBase queryBase, string bsnNumber)
        {
            // Predefined URL components
            string partiesEndpoint = $"https://{((IQueryKlant)this).GetDomain()}/klantinteracties/api/v1/partijen";

            string partyTypeParameter = $"?partijIdentificator__codeSoortObjectId={((IQueryKlant)this).Configuration.AppSettings.Variables.PartyIdentifier()}";
            string partyObjectIdParameter = $"&partijIdentificator__objectId={bsnNumber}";
            const string expandParameter = "&expand=digitaleAdressen";

            // Request URL
            var partiesByTypeIdAndExpand = new Uri($"{partiesEndpoint}{partyTypeParameter}{partyObjectIdParameter}{expandParameter}");

            return (await GetCitizenDetailsV2Async(queryBase, partiesByTypeIdAndExpand))
                .Party(((IQueryKlant)this).Configuration)
                .ConvertToUnified();
        }

        private static async Task<PartyResults> GetCitizenDetailsV2Async(IQueryBase queryBase, Uri citizenByBsnUri)
        {
            return await queryBase.ProcessGetAsync<PartyResults>(
                httpClientType: HttpClientTypes.OpenKlant_v2,
                uri: citizenByBsnUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoCitizenDetails);
        }
        #endregion

        #region Polymorphic (Telemetry)
        /// <inheritdoc cref="IQueryKlant.SendFeedbackAsync"/>
        async Task<ContactMoment> IQueryKlant.SendFeedbackAsync(IQueryBase queryBase, HttpContent body)
        {
            // Predefined URL components
            var klantContactMomentUri = new Uri($"https://{((IQueryKlant)this).GetDomain()}/klantinteracties/api/v1/klantcontacten");

            // Sending the request and getting the response (combined internal logic)
            return await queryBase.ProcessPostAsync<ContactMoment>(
                httpClientType: HttpClientTypes.Telemetry_Klantinteracties,
                uri: klantContactMomentUri,
                body: body,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoFeedbackKlant);
        }

        /// <summary>
        /// Creates a link between the contact moment of the case and subject object in "OpenKlant" Web API service.
        /// </summary>
        /// <returns>
        ///   The JSON response from an external Telemetry Web API service.
        /// </returns>
        /// <exception cref="TelemetryException"/>
        internal static async Task<string> LinkToSubjectObjectAsync(
            IHttpNetworkService networkService, string openKlantDomain, HttpContent body)
        {
            // Predefined URL components
            var subjectObjectUri = new Uri($"https://{openKlantDomain}/klantinteracties/api/v1/onderwerpobjecten");

            // Sending the request
            (bool success, string jsonResponse) = await networkService.PostAsync(
                httpClientType: HttpClientTypes.Telemetry_Klantinteracties,
                uri: subjectObjectUri,
                body: body);

            // Getting the response
            return success ? jsonResponse : throw new TelemetryException(jsonResponse);
        }
        #endregion
    }
}