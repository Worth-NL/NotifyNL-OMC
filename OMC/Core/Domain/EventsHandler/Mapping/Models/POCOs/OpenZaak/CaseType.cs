// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak
{
    /// <summary>
    /// The type of the <see cref="Case"/> retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseType : IJsonSerializable
    {
        /// <summary>
        /// The name of the <see cref="CaseType"/>.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("omschrijving")]
        [JsonPropertyOrder(0)]
        public string Name { get; internal set; } = string.Empty;

        /// <summary>
        /// The description of the <see cref="CaseType"/>.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("omschrijvingGeneriek")]
        [JsonPropertyOrder(1)]
        public string Description { get; internal set; } = string.Empty;

        /// <summary>
        /// The identification of the <see cref="Case"/> in the following format:
        /// <code>
        /// ZAAKTYPE-2023-0000000010
        /// </code>
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("zaaktypeIdentificatie")]
        [JsonPropertyOrder(2)]
        public string Identification { get; internal set; } = string.Empty;

        /// <summary>
        /// Determines whether the <see cref="CaseStatus"/> is final, which means that the <see cref="Case"/> is closed.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("isEindstatus")]
        [JsonPropertyOrder(3)]
        public bool IsFinalStatus { get; internal set; }

        /// <summary>
        /// Determines whether the party (e.g., user or organization) wants to be notified about this certain <see cref="CaseStatus"/> update.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("informeren")]
        [JsonPropertyOrder(4)]
        public bool IsNotificationExpected { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseType"/> struct.
        /// </summary>
        public CaseType()
        {
        }
    }
}