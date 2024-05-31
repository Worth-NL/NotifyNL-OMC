// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Configuration;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "OpenKlant" Web service.
    /// </summary>
    internal interface IQueryKlant
    {
        /// <inheritdoc cref="WebApiConfiguration"/>
        protected internal WebApiConfiguration Configuration { get; set; }

        #region Abstract (Citizen details)
        /// <summary>
        /// Gets the details of a specific citizen from "OpenKlant" Web service.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        internal sealed async Task<CommonPartyData> GetPartyDataAsync(IQueryBase queryBase, string bsnNumber)
        {
            return await GetPartyDataAsync(queryBase, this.Configuration, bsnNumber);
        }

        /// <inheritdoc cref="GetPartyDataAsync(IQueryBase, string)"/>
        protected Task<CommonPartyData> GetPartyDataAsync(
            IQueryBase queryBase, WebApiConfiguration configuration, string bsnNumber);
        #endregion

        #region Helper methods
        /// <summary>
        /// Gets the domain part of the organization-specific (e.g., municipality) "OpenKlant" Web service URI.
        /// <para>
        ///   <code>http(s)://[DOMAIN]/ApiEndpoint</code>
        /// </para>
        /// </summary>
        protected sealed string GetSpecificOpenKlantDomain() => this.Configuration.User.Domain.OpenKlant();
        #endregion
    }
}