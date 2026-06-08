// © 2023, Worth Systems.

using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace ZgwModels.Mapping.Models.POCOs.OpenZaak
{
    /// <summary>
    /// The sensitive data about a single party (e.g., citizen or organization) retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct PartyData : IJsonSerializable
    {
        /// <summary>
        /// The BSN (citizen service number) of the citizen.
        /// </summary>
        [JsonPropertyName("inpBsn")]
        [JsonPropertyOrder(0)]
        public string? BsnNumber { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyData"/> struct.
        /// </summary>
        public PartyData()
        {
        }
    }
}