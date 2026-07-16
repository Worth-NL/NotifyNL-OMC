// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace ZgwModels.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// The additional (expanded) information about the party retrieved from "OpenKlant" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (2.0) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IJsonSerializable" />
    public struct Expansion : IJsonSerializable
    {
        /// <inheritdoc cref="DigitalAddressLong"/>
        [JsonRequired]
        [JsonPropertyName("digitaleAdressen")]
        [JsonPropertyOrder(0)]
        public List<DigitalAddressLong> DigitalAddresses { get; set; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="Expansion"/> struct.
        /// </summary>
        public Expansion()
        {
        }
    }
}