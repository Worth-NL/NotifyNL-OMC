// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak.Decision
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
        [JsonInclude]
        [JsonPropertyName("omschrijving")]
        [JsonPropertyOrder(0)]
        public string Name { get; internal set; } = string.Empty;

        /// <summary>
        /// The description of the <see cref="DecisionType"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("omschrijvinggeneriek")]
        [JsonPropertyOrder(1)]
        public string Description { get; internal set; } = string.Empty;

        /// <summary>
        /// The category of the <see cref="DecisionType"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("besluitcategorie")]
        [JsonPropertyOrder(2)]
        public string Category { get; internal set; } = string.Empty;

        /// <summary>
        /// The deadline date to respond for this <see cref="DecisionType"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("reactietermijn")]
        [JsonPropertyOrder(3)]
        public DateOnly ResponseDeadline { get; internal set; }

        /// <summary>
        /// The indication of the publication.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("publicatieindicatie")]
        [JsonPropertyOrder(4)]
        public string PublicationIndicator { get; internal set; } = string.Empty;

        /// <summary>
        /// The text of the publication.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("publicatietekst")]
        [JsonPropertyOrder(5)]
        public string PublicationText { get; internal set; } = string.Empty;

        /// <summary>
        /// The deadline date of the publication.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("publicatietermijn")]
        [JsonPropertyOrder(6)]
        public DateOnly PublicationDeadline { get; internal set; }

        /// <summary>
        /// The explanation of the <see cref="DecisionType"/>.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("toelichting")]
        [JsonPropertyOrder(7)]
        public string Explanation { get; internal set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecisionType"/> struct.
        /// </summary>
        public DecisionType()
        {
        }
    }
}