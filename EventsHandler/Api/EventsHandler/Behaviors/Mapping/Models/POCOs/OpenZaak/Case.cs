// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak
{
    /// <summary>
    /// The case retrieved from "OpenZaak" Web service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Case : IJsonSerializable
    {
        /// <summary>
        /// Gets the description of the case which is an equivalent to the case name.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("omschrijving")]
        [JsonPropertyOrder(1)]
        public string Name { get; internal set; } = string.Empty;

        /// <summary>
        /// Gets the identification of the case which is an equivalent to the case number.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("identificatie")]
        [JsonPropertyOrder(2)]
        public string Identification { get; internal set; } = string.Empty;

        // TODO: "zaaktype" might be used as well

        /// <summary>
        /// Initializes a new instance of the <see cref="Case"/> struct.
        /// </summary>
        public Case()
        {
        }
    }
}