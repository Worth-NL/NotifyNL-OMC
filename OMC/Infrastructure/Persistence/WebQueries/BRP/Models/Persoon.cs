// © 2026, Worth Systems.

using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace WebQueries.BRP.Models
{
    /// <summary>
    /// A single person as returned by the BRP API Personen.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Persoon : IJsonSerializable
    {
        /// <summary>
        /// The BSN (Burgerservicenummer) of the person.
        /// </summary>
        [JsonPropertyName("burgerservicenummer")]
        [JsonPropertyOrder(0)]
        public string? Bsn { get; set; }

        /// <summary>
        /// The name data of the person.
        /// </summary>
        [JsonPropertyName("naam")]
        [JsonPropertyOrder(1)]
        public NaamPersoon? Naam { get; set; }

        /// <summary>
        /// The gender of the person.
        /// </summary>
        [JsonPropertyName("geslacht")]
        [JsonPropertyOrder(2)]
        public Waardetabel? Geslacht { get; set; }

        /// <summary>
        /// The "Adressering" informatieproduct: everything needed to send this person a letter.
        /// </summary>
        [JsonPropertyName("adressering")]
        [JsonPropertyOrder(3)]
        public Adressering? Adressering { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Persoon"/> struct.
        /// </summary>
        public Persoon()
        {
        }
    }
}
