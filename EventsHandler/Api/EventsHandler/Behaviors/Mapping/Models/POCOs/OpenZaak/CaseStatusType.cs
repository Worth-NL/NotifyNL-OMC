// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak
{
    /// <summary>
    /// A type of the <see cref="CaseStatus"/> retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseStatusType : IJsonSerializable
    {
        /// <summary>
        /// The name of the updated status.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("omschrijving")]
        [JsonPropertyOrder(0)]
        public string Description { get; internal set; } = string.Empty;

        /// <summary>
        /// Determines whether the status is the final one => The <see cref="Case"/> was closed.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("isEindstatus")]
        [JsonPropertyOrder(1)]
        public bool IsFinalStatus { get; internal set; }

        /// <summary>
        /// Determines whether the party (e.g., user or organization) wants to be notified about
        /// this certain type of notification.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("informeren")]
        [JsonPropertyOrder(2)]
        public bool IsNotificationExpected { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseStatusType"/> struct.
        /// </summary>
        public CaseStatusType()
        {
        }
    }
}