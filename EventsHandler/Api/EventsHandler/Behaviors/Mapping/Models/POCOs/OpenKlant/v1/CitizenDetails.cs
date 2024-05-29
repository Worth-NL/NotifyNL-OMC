// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v1
{
    /// <summary>
    /// The details about the citizen retrieved from "OpenKlant" (1.0) Web service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CitizenDetails : IJsonSerializable
    {
        /// <summary>
        /// The number of received results.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("count")]
        [JsonPropertyOrder(0)]
        public int Count { get; internal set; }

        /// <inheritdoc cref="CitizenData"/>
        [JsonInclude]
        [JsonPropertyName("results")]
        [JsonPropertyOrder(1)]
        public List<CitizenData> Results { get; internal set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CitizenDetails"/> struct.
        /// </summary>
        public CitizenDetails()
        {
        }

        /// <summary>
        /// Gets the <see cref="CitizenData"/>.
        /// </summary>
        /// <value>
        ///   The data of a single citizen.
        /// </value>
        internal readonly CitizenData Citizen => this.Results[^1];
    }
}