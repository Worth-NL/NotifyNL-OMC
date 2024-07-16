// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Converters;
using EventsHandler.Constants;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Enums.Objecten
{
    /// <summary>
    /// The status of the task from "Objecten" Web API service.
    /// </summary>
    [JsonConverter(typeof(SafeJsonStringEnumMemberConverter<TaskStatuses>))]
    public enum TaskStatuses
    {
        /// <summary>
        /// Default value.
        /// </summary>
        [JsonPropertyName(DefaultValues.Models.DefaultEnumValueName)]
        Unknown = 0,

        /// <summary>
        /// The task is open (e.g., newly assigned or re-opened).
        /// </summary>
        [JsonPropertyName("open")]
        Open = 1,

        /// <summary>
        /// The task is closed (e.g., already processed, declined, manually closed).
        /// </summary>
        [JsonPropertyName("gesloten")]
        Closed = 2
    }
}