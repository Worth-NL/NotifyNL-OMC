﻿// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Properties;
using EventsHandler.Services.Settings.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak.v2
{
    /// <summary>
    /// The roles of the case retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenZaak" (1.0) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseRoles : IJsonSerializable
    {
        /// <summary>
        /// The number of received results.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("count")]
        [JsonPropertyOrder(0)]
        public int Count { get; internal set; }
        
        /// <summary>
        /// The collection of:
        /// <inheritdoc cref="CaseRole"/>
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("results")]
        [JsonPropertyOrder(1)]
        public List<CaseRole> Results { get; internal set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseRoles"/> struct.
        /// </summary>
        public CaseRoles()
        {
        }

        /// <summary>
        /// Gets the <see cref="CitizenData"/> with matching "initiator role" set.
        /// </summary>
        /// <returns>
        ///   The data of a single citizen.
        /// </returns>
        /// <exception cref="HttpRequestException"/>
        internal readonly CitizenData Citizen(WebApiConfiguration configuration)
        {
            // Response does not contain any results (check notification or project configuration)
            if (this.Results.IsNullOrEmpty())
            {
                throw new HttpRequestException(Resources.HttpRequest_ERROR_EmptyCaseRoles);
            }

            CitizenData? citizen = null;

            foreach (CaseRole caseRole in this.Results)
            {
                if (caseRole.InitiatorRole != configuration.AppSettings.Variables.InitiatorRole())
                {
                    continue;
                }

                // First initiator result was found
                if (citizen == null)
                {
                    citizen = caseRole.Citizen;
                }
                // Multiple initiator results were found (no way to determine who is the initiator)
                else
                {
                    throw new HttpRequestException(Resources.HttpRequest_ERROR_MultipleInitiatorRoles);
                }
            }

            // Zero initiator results were found (there is no initiator)
            if (citizen == null)
            {
                throw new HttpRequestException(Resources.HttpRequest_ERROR_MissingInitiatorRole);
            }

            return citizen.Value;
        }
    }
}