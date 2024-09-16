// © 2023, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Converters;
using EventsHandler.Mapping.Models.POCOs.Objecten.Message;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Enums.NotificatieApi
{
    /// <summary>
    /// The name of the notification resource.
    /// </summary>
    [JsonConverter(typeof(SafeJsonStringEnumMemberConverter<Resources>))]
    public enum Resources
    {
        /// <summary>
        /// The default value.
        /// </summary>
        [JsonPropertyName(DefaultValues.Models.DefaultEnumValueName)]
        Unknown = 0,

        /// <summary>
        /// The "case" resource.
        /// </summary>
        [JsonPropertyName("zaak")]
        Case = 1,

        /// <summary>
        /// The "status" (e.g., case status) resource.
        /// </summary>
        [JsonPropertyName("status")]
        Status = 2,

        /// <summary>
        /// The "object" (task, message...) resource.
        /// </summary>
        [JsonPropertyName("object")]
        Object = 3,

        /// <summary>
        /// The "decision" resource.
        /// </summary>
        [JsonPropertyName("besluitinformatieobject")]
        Decision = 4
    }
}