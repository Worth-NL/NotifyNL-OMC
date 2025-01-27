// © 2024, Worth Systems.

using Common.Settings.Configuration;
using System.Text.Json;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataSending.Interfaces;
using WebQueries.Exceptions;
using WebQueries.Versioning.Interfaces;
using ZhvModels.Mapping.Models.POCOs.OpenKlant;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;

namespace WebQueries.DataQuerying.Strategies.Queries.OpenKlant.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "OpenKlant" Web API service.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    /// <seealso cref="IDomain"/>
    public interface IQueryKlant : IVersionDetails, IDomain
    {
        /// <inheritdoc cref="OmcConfiguration"/>
        protected internal OmcConfiguration Configuration { get; set; }

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
        /// <param name="caseIdentifier">The Case identifier used to select the digital address with the highest priority if match is found.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal Task<CommonPartyData> TryGetPartyDataAsync(IQueryBase queryBase, string bsnNumber, string? caseIdentifier = null);

        /// <summary>
        /// <inheritdoc cref="TryGetPartyDataAsync(IQueryBase, string, string?)"/>
        /// </summary>
        /// <remarks>
        ///   The method used to obtain company data.
        /// </remarks>
        /// <param name="queryBase"><inheritdoc cref="IQueryBase" path="/summary"/></param>
        /// <param name="involvedPartyUri">The <see cref="Uri"/> to get the involved organization party.</param>
        /// <param name="caseIdentifier">The identifier of the case.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal Task<CommonPartyData> TryGetPartyDataAsync(IQueryBase queryBase, Uri involvedPartyUri, string? caseIdentifier = null);
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
        internal Task<HttpRequestResponse> LinkCaseToContactMomentAsync(IHttpNetworkService networkService, string jsonBody);

        /// <summary>
        /// Links the <see cref="ContactMoment"/> with the <see cref="CommonPartyData"/>.
        /// </summary>
        /// <param name="networkService"><inheritdoc cref="IHttpNetworkService" path="/summary"/></param>
        /// <param name="jsonBody">The JSON body to be passed.</param>
        /// <returns>
        ///   The response from an external Web API service.
        /// </returns>
        /// <exception cref="KeyNotFoundException"/>
        internal Task<HttpRequestResponse> LinkPartyToContactMomentAsync(IHttpNetworkService networkService, string jsonBody);
        #endregion

        #region Polymorphic (Domain)
        /// <inheritdoc cref="IDomain.GetDomain"/>
        string IDomain.GetDomain() => this.Configuration.ZGW.Endpoint.OpenKlant();
        #endregion
    }
}