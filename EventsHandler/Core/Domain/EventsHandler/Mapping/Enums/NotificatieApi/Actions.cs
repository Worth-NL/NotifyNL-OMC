// © 2023, Worth Systems.

using Common.Constants;
using EventsHandler.Mapping.Converters;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Enums.NotificatieApi
{
    /// <summary>
    /// The action taken by the publishing Web API service.
    /// </summary>
    /// <remarks>
    /// The publishing API specifies the allowed actions.
    /// </remarks>
    [JsonConverter(typeof(SafeJsonStringEnumMemberConverter<Actions>))]
    public enum Actions
    {
        /// <summary>
        /// The default value.
        /// </summary>
        [JsonPropertyName(DefaultValues.Models.DefaultEnumValueName)]
        Unknown = 0,

        /// <summary>
        /// Create action.
        /// </summary>
        [JsonPropertyName("create")]
        Create = 1,

        /// <summary>
        /// Update action.
        /// </summary>
        [JsonPropertyName("update")]
        Update = 2,

        /// <summary>
        /// Destroy action.
        /// </summary>
        [JsonPropertyName("destroy")]
        Destroy = 3
    }
}