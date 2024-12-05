// © 2024, Worth Systems.

using Common.Constants;
using Common.Enums.Converters;
using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.POCOs.Objecten.Task;

namespace ZhvModels.Mapping.Enums.Objecten
{
    /// <summary>
    /// The status of the <see cref="CommonTaskData.Status"/> from "Objecten" Web API service.
    /// </summary>
    [JsonConverter(typeof(SafeJsonStringEnumMemberConverter<TaskStatuses>))]
    public enum TaskStatuses
    {
        /// <summary>
        /// The default value.
        /// </summary>
        [JsonPropertyName(CommonValues.Default.Models.DefaultEnumValueName)]
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