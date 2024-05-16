// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces;
using EventsHandler.Services.DataReceiving.Enums;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.v1
{
    /// <inheritdoc cref="IQueryKlant"/>
    internal sealed class QueryKlant : IQueryKlant
    {
        private readonly WebApiConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryKlant"/> class.
        /// </summary>
        public QueryKlant(WebApiConfiguration configuration)
        {
            this._configuration = configuration;
        }
        
        /// <inheritdoc cref="IQueryKlant.GetCitizenDetailsAsync(IQueryBase, string)"/>
        async Task<CitizenDetails> IQueryKlant.GetCitizenDetailsAsync(IQueryBase queryBase, string bsnNumber)
        {
            // Predefined URL components
            string citizensEndpoint = $"https://{GetSpecificOpenKlantDomain()}/klanten/api/v1/klanten";
            
            // Request URL
            Uri citizenByBsnUri = new($"{citizensEndpoint}?subjectNatuurlijkPersoon__inpBsn={bsnNumber}");

            return await queryBase.ProcessGetAsync<CitizenDetails>(HttpClientTypes.Data, citizenByBsnUri, Resources.HttpRequest_ERROR_NoCitizenDetails);
        }
        
        #region Helper methods
        /// <summary>
        /// Gets the domain part of the organization-specific (e.g., municipality) "OpenKlant" Web service URI.
        /// <para>
        ///   <code>http(s)://[DOMAIN]/ApiEndpoint</code>
        /// </para>
        /// </summary>
        private string GetSpecificOpenKlantDomain() => this._configuration.User.Domain.OpenKlant();
        #endregion
    }
}