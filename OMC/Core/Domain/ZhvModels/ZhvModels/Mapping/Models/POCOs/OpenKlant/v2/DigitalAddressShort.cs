// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// The ID of the digital address retrieved from "OpenKlant" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (2.0) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IJsonSerializable" />
    public struct DigitalAddressShort : IJsonSerializable
    {
        /// <summary>
        /// The UUID / GUID of the digital address.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("uuid")]
        [JsonPropertyOrder(0)]
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalAddressShort"/> struct.
        /// </summary>
        public DigitalAddressShort()
        {
        }
    }
}