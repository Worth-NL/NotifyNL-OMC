// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak
{
    /// <summary>
    /// The type of the <see cref="CaseStatus"/> retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseType : IJsonSerializable
    {
        /// <summary>
        /// The description of the <see cref="CaseType"/> which is an equivalent of its name.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("omschrijving")]
        [JsonPropertyOrder(0)]
        public string Name { get; internal set; } = string.Empty;

        /// <summary>
        /// Determines whether the <see cref="CaseStatus"/> is final, which means that the <see cref="Case"/> is closed.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("isEindstatus")]
        [JsonPropertyOrder(1)]
        public bool IsFinalStatus { get; internal set; }

        /// <summary>
        /// Determines whether the party (e.g., user or organization) wants to be notified about
        /// this certain <see cref="CaseStatus"/> update.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("informeren")]
        [JsonPropertyOrder(2)]
        public bool IsNotificationExpected { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseType"/> struct.
        /// </summary>
        public CaseType()
        {
        }
    }
}