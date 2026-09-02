// © 2026, Worth Systems.

using Common.Constants;
using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace ZgwModels.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// The response of creating a new party (e.g., citizen) in "OpenKlant" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (2.0) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="PartyResult"/>
    /// <seealso cref="IJsonSerializable"/>
    public struct PartyCreationResult : IJsonSerializable
    {
        /// <inheritdoc cref="CommonPartyData.Uri"/>
        [JsonRequired]
        [JsonPropertyName("url")]
        [JsonPropertyOrder(0)]
        public Uri Uri { get; set; } = CommonValues.Default.Models.EmptyUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyCreationResult"/> struct.
        /// </summary>
        public PartyCreationResult()
        {
        }
    }
}
