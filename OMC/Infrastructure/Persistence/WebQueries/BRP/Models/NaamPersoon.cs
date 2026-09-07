// © 2026, Worth Systems.

using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace WebQueries.BRP.Models
{
    /// <summary>
    /// Name data of a person, as returned by the BRP API Personen "naam" field.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct NaamPersoon : IJsonSerializable
    {
        /// <summary>
        /// The given names (voornamen), separated by spaces.
        /// </summary>
        [JsonPropertyName("voornamen")]
        [JsonPropertyOrder(0)]
        public string? Voornamen { get; set; }

        /// <summary>
        /// The prefix preceding the surname (e.g., "van", "de").
        /// </summary>
        [JsonPropertyName("voorvoegsel")]
        [JsonPropertyOrder(1)]
        public string? Voorvoegsel { get; set; }

        /// <summary>
        /// The surname of the person.
        /// </summary>
        [JsonPropertyName("geslachtsnaam")]
        [JsonPropertyOrder(2)]
        public string? Geslachtsnaam { get; set; }

        /// <summary>
        /// The initials of the person, derived from <see cref="Voornamen"/>.
        /// </summary>
        [JsonPropertyName("voorletters")]
        [JsonPropertyOrder(3)]
        public string? Voorletters { get; set; }

        /// <summary>
        /// The combination of predicate, given names, noble title, prefix, and surname
        /// (without any partner's name).
        /// </summary>
        [JsonPropertyName("volledigeNaam")]
        [JsonPropertyOrder(4)]
        public string? VolledigeNaam { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NaamPersoon"/> struct.
        /// </summary>
        public NaamPersoon()
        {
        }
    }
}
