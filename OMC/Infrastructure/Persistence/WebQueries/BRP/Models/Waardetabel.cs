// © 2026, Worth Systems.

using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace WebQueries.BRP.Models
{
    /// <summary>
    /// A generic "value table" entry used throughout the BRP API (e.g., for <c>geslacht</c> or <c>land</c>):
    /// a code paired with its human-readable description.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Waardetabel : IJsonSerializable
    {
        /// <summary>
        /// The code from the underlying national value table.
        /// </summary>
        [JsonPropertyName("code")]
        [JsonPropertyOrder(0)]
        public string? Code { get; set; }

        /// <summary>
        /// The human-readable description of <see cref="Code"/>.
        /// </summary>
        [JsonPropertyName("omschrijving")]
        [JsonPropertyOrder(1)]
        public string? Omschrijving { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Waardetabel"/> struct.
        /// </summary>
        public Waardetabel()
        {
        }
    }
}
