// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak
{
    /// <summary>
    /// The sensitive data about a single citizen ("burger") retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CitizenData : IJsonSerializable
    {
        /// <summary>
        /// The BSN number of the citizen.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("inpBsn")]
        [JsonPropertyOrder(0)]
        public string BsnNumber { get; internal set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="CitizenData"/> struct.
        /// </summary>
        public CitizenData()
        {
        }
    }
}