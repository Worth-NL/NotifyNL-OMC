// © 2024, Worth Systems.

using EventsHandler.Exceptions;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.DataSending.Responses;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Versioning.Interfaces;
using System.Text.Json;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "OpenKlant" Web API service.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    /// <seealso cref="IDomain"/>
    internal interface IQueryKlant : IVersionDetails, IDomain
    {
        /// <inheritdoc cref="WebApiConfiguration"/>
        protected internal WebApiConfiguration Configuration { get; set; }

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "OpenKlant";

        #region Abstract (Citizen details)
        /// <summary>
        /// Gets the details of a specific citizen from "OpenKlant" Web API service.
        /// </summary>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="bsnNumber">The BSN (Citizen Service Number).</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal Task<CommonPartyData> TryGetPartyDataAsync(IQueryBase queryBase, string bsnNumber);
        #endregion

        #region Abstract (Telemetry)
        /// <summary>
        /// Creates the <see cref="ContactMoment"/> in the register from "OpenKlant" Web API service.
        /// </summary>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="jsonBody">The JSON body to be passed.</param>
        /// <returns>
        ///   The response from "OpenZaak" Web API service mapped into a <see cref="ContactMoment"/> object.
        /// </returns>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="TelemetryException"/>
        /// <exception cref="JsonException"/>
        internal Task<ContactMoment> CreateContactMomentAsync(IQueryBase queryBase, string jsonBody);

        /// <summary>
        /// Links the <see cref="ContactMoment"/> with the <see cref="Case"/>.
        /// </summary>
        /// <param name="networkService"><inheritdoc cref="IHttpNetworkService" path="/summary"/></param>
        /// <param name="jsonBody">The JSON body to be passed.</param>
        /// <returns>
        ///   The response from an external Web API service.
        /// </returns>
        /// <exception cref="KeyNotFoundException"/>
        internal Task<RequestResponse> LinkCaseToContactMomentAsync(IHttpNetworkService networkService, string jsonBody);

        /// <summary>
        /// Links the <see cref="ContactMoment"/> with the <see cref="CommonPartyData"/>.
        /// </summary>
        /// <param name="networkService"><inheritdoc cref="IHttpNetworkService" path="/summary"/></param>
        /// <param name="jsonBody">The JSON body to be passed.</param>
        /// <returns>
        ///   The response from an external Web API service.
        /// </returns>
        /// <exception cref="KeyNotFoundException"/>
        internal Task<RequestResponse> LinkCustomerToContactMomentAsync(IHttpNetworkService networkService, string jsonBody);
        #endregion

        #region Polymorphic (Domain)
        /// <inheritdoc cref="IDomain.GetDomain"/>
        string IDomain.GetDomain() => this.Configuration.User.Domain.OpenKlant();
        #endregion
    }
}