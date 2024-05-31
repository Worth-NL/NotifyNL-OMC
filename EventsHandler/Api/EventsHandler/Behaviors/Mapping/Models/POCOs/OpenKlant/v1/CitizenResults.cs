// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v1
{
    /// <summary>
    /// The results about the citizens retrieved from "OpenKlant" Web service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (1.0) Web service.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public struct CitizenResults : IJsonSerializable
    {
        /// <summary>
        /// The number of received results.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("count")]
        [JsonPropertyOrder(0)]
        public int Count { get; internal set; }

        /// <inheritdoc cref="CitizenResult"/>
        [JsonInclude]
        [JsonPropertyName("results")]
        [JsonPropertyOrder(1)]
        public List<CitizenResult> Results { get; internal set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CitizenResults"/> struct.
        /// </summary>
        public CitizenResults()
        {
        }

        /// <summary>
        /// Gets the <see cref="CitizenResult"/>.
        /// </summary>
        /// <value>
        ///   The data of a single citizen.
        /// </value>
        internal readonly CitizenResult Citizen => this.Results[^1];  // TODO: Exception
    }
}