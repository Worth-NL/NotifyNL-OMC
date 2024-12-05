// © 2024, Worth Systems.

using Common.Settings.Configuration;
using EventsHandler.Exceptions;
using EventsHandler.Models.Responses.Sending;
using EventsHandler.Services.DataQuerying.Strategies.Interfaces;
using EventsHandler.Services.DataQuerying.Strategies.Queries;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.Versioning.Interfaces;
using System.Text.Json;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;

namespace EventsHandler.Services.DataQuerying.Strategies.Queries.OpenKlant.Interfaces
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

        #region Abstract (Party data)
        /// <summary>
        /// Gets the details of a specific party (e.g., citizen or organization) from "OpenKlant" Web API service.
        /// </summary>
        /// <remarks>
        ///   The method used to obtain citizen data.
        /// </remarks>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="bsnNumber">The BSN (Citizen Service Number) to get citizen party.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal Task<CommonPartyData> TryGetPartyDataAsync(IQueryBase queryBase, string bsnNumber);

        /// <summary>
        /// <inheritdoc cref="TryGetPartyDataAsync(IQueryBase, string)"/>
        /// </summary>
        /// <remarks>
        ///   The method used to obtain company data.
        /// </remarks>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="involvedPartyUri">The <see cref="Uri"/> to get the involved organization party.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal Task<CommonPartyData> TryGetPartyDataAsync(IQueryBase queryBase, Uri involvedPartyUri);
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
        /// Links the <see cref="ContactMoment"/> with the <see cref="ZhvModels.Mapping.Models.POCOs.OpenZaak.Case"/>.
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
        string IDomain.GetDomain() => Configuration.ZGW.Endpoint.OpenKlant();
        #endregion
    }
}