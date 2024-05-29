﻿// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Configuration;
using EventsHandler.Properties;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak.v2
{
    /// <summary>
    /// The roles of the case retrieved from "OpenZaak" (2.0) Web service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseRoles : IJsonSerializable
    {
        /// <summary>
        /// The number of received results.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("count")]
        [JsonPropertyOrder(0)]
        public int Count { get; internal set; }

        /// <inheritdoc cref="CaseRole"/>
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
        /// Gets the <see cref="CitizenData"/>.
        /// </summary>
        /// <value>
        ///   The data of a single citizen (matching to the internal criteria).
        /// </value>
        internal readonly CitizenData Citizen(WebApiConfiguration configuration)
        {
            // Response does not contain any results (check notification or project configuration)
            if (Results.IsNullOrEmpty())
            {
                throw new HttpRequestException(Resources.HttpRequest_ERROR_EmptyCaseRoles);
            }

            CitizenData? citizen = null;

            foreach (CaseRole caseRole in Results)
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