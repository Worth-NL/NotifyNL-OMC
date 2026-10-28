// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace ZgwModels.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// The details of digital address retrieved from "OpenKlant" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (2.0) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IJsonSerializable" />
    public struct DigitalAddressLong : IJsonSerializable
    {
        /// <summary>
        /// The UUID / GUID of the digital address.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("uuid")]
        [JsonPropertyOrder(0)]
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// The value of the digital address.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("adres")]
        [JsonPropertyOrder(1)]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// The type of the digital address.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("soortDigitaalAdres")]
        [JsonPropertyOrder(2)]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// The reference of the digital address.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("referentie")]
        [JsonPropertyOrder(3)]
        public string Reference { get; set; } = string.Empty;

        /// <summary>
        /// Whether this is the party's default address of its kind (at most one e-mail and one phone number per party).
        /// </summary>
        /// <remarks>
        ///   Not [JsonRequired]: OpenKlant only has it since 2.4.0.
        /// </remarks>
        [JsonPropertyName("isStandaardAdres")]
        [JsonPropertyOrder(4)]
        public bool IsStandardAddress { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalAddressLong"/> struct.
        /// </summary>
        public DigitalAddressLong()
        {
        }
    }
}