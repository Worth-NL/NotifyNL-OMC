// © 2023, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Converters;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Enums.NotificatieApi
{
    /// <summary>
    /// The resource name that the notification is about.
    /// </summary>
    [JsonConverter(typeof(SafeJsonStringEnumMemberConverter<Resources>))]
    public enum Resources
    {
        /// <summary>
        /// Default value.
        /// </summary>
        [JsonPropertyName(DefaultValues.Models.DefaultEnumValueName)]
        Unknown = 0,

        /// <summary>
        /// Case resource.
        /// </summary>
        [JsonPropertyName("zaak")]
        Case = 1,

        /// <summary>
        /// Object resource.
        /// </summary>
        [JsonPropertyName("object")]
        Object = 2,

        /// <summary>
        /// Case status.
        /// </summary>
        [JsonPropertyName("status")]
        Status = 3,

        /// <summary>
        /// Decision resource.
        /// </summary>
        [JsonPropertyName("besluitinformatieobject")]
        Decision = 4
    }
}