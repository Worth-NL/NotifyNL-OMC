// © 2025, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;
using ZhvModels.Mapping.Models.POCOs.OpenKlant.v2;

namespace ZhvModels.Mapping.Models.POCOs.OpenKlant
{
    /// <summary>
    /// The sensitive data about the party (e.g., citizen, organization) retrieved from "OpenKlant" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (2.0) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="CommonPartyData"/>
    /// <seealso cref="IJsonSerializable"/>
    public struct LetterAddress : IJsonSerializable
    {
        /// <inheritdoc cref="CommonPartyData.Uri"/>
        [JsonRequired]
        [JsonPropertyName("Street")]
        [JsonPropertyOrder(0)]
        public string Street { get; set; } = String.Empty;

        /// <inheritdoc cref="DigitalAddressShort"/>
        [JsonRequired]
        [JsonPropertyName("Number")]
        [JsonPropertyOrder(1)]
        public string Number { get; set; }

        /// <inheritdoc cref="PartyIdentification"/>
        [JsonRequired]
        [JsonPropertyName("Zip")]
        [JsonPropertyOrder(2)]
        public string Zip { get; set; }

        /// <inheritdoc cref="v2.Expansion"/>
        [JsonRequired]
        [JsonPropertyName("City")]
        [JsonPropertyOrder(3)]
        public string City { get; set; }

        /// <summary>
        /// The identification details of the subject.
        /// </summary>
        [JsonPropertyName("Country")]
        [JsonPropertyOrder(15)]
        public string Country { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyResult"/> struct.
        /// </summary>
        public LetterAddress(string number, string zip, string city, string country)
        {
            Number = number;
            Zip = zip;
            City = city;
            Country = country;
        }
    }
}
