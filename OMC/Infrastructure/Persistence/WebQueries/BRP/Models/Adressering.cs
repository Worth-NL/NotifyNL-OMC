// © 2026, Worth Systems.

using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace WebQueries.BRP.Models
{
    /// <summary>
    /// The "Adressering" informatieproduct of the BRP API Personen: everything needed to send a
    /// letter or mailing to a person, ready-made (form of address, salutation, and envelope-window
    /// formatted address lines), without needing to derive or reformat any of it.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Adressering : IJsonSerializable
    {
        /// <summary>
        /// The first address line: street and house number.
        /// </summary>
        [JsonPropertyName("adresregel1")]
        [JsonPropertyOrder(0)]
        public string? Adresregel1 { get; set; }

        /// <summary>
        /// The second address line: city, possibly combined with the postcode.
        /// </summary>
        [JsonPropertyName("adresregel2")]
        [JsonPropertyOrder(1)]
        public string? Adresregel2 { get; set; }

        /// <summary>
        /// The third address line: optional geographic area(s), only for a foreign address.
        /// </summary>
        [JsonPropertyName("adresregel3")]
        [JsonPropertyOrder(2)]
        public string? Adresregel3 { get; set; }

        /// <summary>
        /// The country, only present for a foreign address.
        /// </summary>
        [JsonPropertyName("land")]
        [JsonPropertyOrder(3)]
        public Waardetabel? Land { get; set; }

        /// <summary>
        /// Indicates it has been established that the person no longer lives at the registered address.
        /// </summary>
        [JsonPropertyName("indicatieVastgesteldVerblijftNietOpAdres")]
        [JsonPropertyOrder(4)]
        public bool? IndicatieVastgesteldVerblijftNietOpAdres { get; set; }

        /// <summary>
        /// The salutation to use in a letter addressed to this person (e.g., "Geachte heer/mevrouw ...").
        /// </summary>
        [JsonPropertyName("aanhef")]
        [JsonPropertyOrder(5)]
        public string? Aanhef { get; set; }

        /// <summary>
        /// The composed name and form of address to use when communicating with the person.
        /// </summary>
        [JsonPropertyName("aanschrijfwijze")]
        [JsonPropertyOrder(6)]
        public AanschrijfwijzeDetails? Aanschrijfwijze { get; set; }

        /// <summary>
        /// The name to use when referring to the person in running text (e.g., inside a letter's body).
        /// </summary>
        [JsonPropertyName("gebruikInLopendeTekst")]
        [JsonPropertyOrder(7)]
        public string? GebruikInLopendeTekst { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Adressering"/> struct.
        /// </summary>
        public Adressering()
        {
        }

        /// <summary>
        /// The composed name and form of address of a person.
        /// </summary>
        public struct AanschrijfwijzeDetails
        {
            /// <summary>
            /// The composed name to use.
            /// </summary>
            [JsonPropertyName("naam")]
            public string? Naam { get; set; }

            /// <summary>
            /// A line to place above <see cref="Naam"/>; only present for persons with a noble title or predicate.
            /// </summary>
            [JsonPropertyName("aanspreekvorm")]
            public string? Aanspreekvorm { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="AanschrijfwijzeDetails"/> struct.
            /// </summary>
            public AanschrijfwijzeDetails()
            {
            }
        }
    }
}
