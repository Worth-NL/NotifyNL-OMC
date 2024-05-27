// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak.v1
{
    /// <summary>
    /// The roles of the case retrieved from "OpenZaak" (1.0) Web service.
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
        ///   The data of a single citizen.
        /// </value>
        internal readonly CitizenData Citizen => this.Results[^1].Citizen;  // NOTE: The requirement is to get only the last result from many possible
    }
}