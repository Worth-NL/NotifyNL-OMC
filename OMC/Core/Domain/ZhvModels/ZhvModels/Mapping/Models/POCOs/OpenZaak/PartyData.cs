// © 2023, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.OpenZaak
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
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("inpBsn")]
        [JsonPropertyOrder(0)]
        public string BsnNumber { get; public set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyData"/> struct.
        /// </summary>
        public PartyData()
        {
        }
    }
}