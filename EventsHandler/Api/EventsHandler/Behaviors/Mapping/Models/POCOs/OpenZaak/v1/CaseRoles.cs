// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Properties;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak.v1
{
    /// <summary>
    /// The roles of the case retrieved from "OpenZaak" Web service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenZaak" (1.0) Web service.
    /// </remarks>
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
        /// Gets the most recent (last) <see cref="CitizenData"/>.
        /// </summary>
        /// <value>
        ///   The data of a single citizen.
        /// </value>
        internal readonly CitizenData Citizen
        {
            get
            {
                if (Results.IsNullOrEmpty())
                {
                    throw new HttpRequestException(Resources.HttpRequest_ERROR_EmptyCaseRoles);
                }

                return this.Results[^1].Citizen;  // NOTE: The requirement is to get only the last result from many possible
            }
        }
    }
}