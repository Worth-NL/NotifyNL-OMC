// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision
{
    /// <summary>
    /// The type of the <see cref="Decision"/> retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct DecisionType : IJsonSerializable
    {
        /// <summary>
        /// The name of the <see cref="DecisionType"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("omschrijving")]
        [JsonPropertyOrder(0)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The description of the <see cref="DecisionType"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("omschrijvingGeneriek")]
        [JsonPropertyOrder(1)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The category of the <see cref="DecisionType"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("besluitcategorie")]
        [JsonPropertyOrder(2)]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// The indication of the publication.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("publicatieIndicatie")]
        [JsonPropertyOrder(3)]
        public bool PublicationIndicator { get; set; }

        /// <summary>
        /// The text of the publication.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("publicatietekst")]
        [JsonPropertyOrder(4)]
        public string PublicationText { get; set; } = string.Empty;

        /// <summary>
        /// The explanation of the <see cref="DecisionType"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("toelichting")]
        [JsonPropertyOrder(5)]
        public string Explanation { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecisionType"/> struct.
        /// </summary>
        public DecisionType()
        {
        }
    }
}