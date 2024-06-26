﻿// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Behaviors.Versioning;
using EventsHandler.Configuration;
using EventsHandler.Exceptions;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using System.Text.Json;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.OpenKlant.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "OpenKlant" Web API service.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    internal interface IQueryKlant : IVersionDetails
    {
        /// <inheritdoc cref="WebApiConfiguration"/>
        protected internal WebApiConfiguration Configuration { get; set; }

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "OpenKlant";

        #region Abstract (Citizen details)
        /// <summary>
        /// Gets the details of a specific citizen from "OpenKlant" Web API service.
        /// </summary>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException"/>
        internal Task<CommonPartyData> GetPartyDataAsync(IQueryBase queryBase, string bsnNumber);
        #endregion
        
        #region Abstract (Telemetry)
        /// <summary>
        /// Sends the completion feedback to "OpenKlant" Web API service.
        /// </summary>
        /// <exception cref="KeyNotFoundException"/>
        /// <exception cref="TelemetryException"/>
        /// <exception cref="JsonException"/>
        internal Task<ContactMoment> SendFeedbackAsync(IQueryBase queryBase, HttpContent body);
        #endregion

        #region Domain
        /// <summary>
        /// Gets the domain part of the organization-specific (e.g., municipality) "OpenKlant" Web API service URI:
        /// <code>
        ///   http(s)://[DOMAIN]/ApiEndpoint
        /// </code>
        /// </summary>
        /// <exception cref="KeyNotFoundException"/>
        internal sealed string GetSpecificOpenKlantDomain() => this.Configuration.User.Domain.OpenKlant();
        #endregion
    }
}