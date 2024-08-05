// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Converters;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Enums.OpenZaak
{
    /// <summary>
    /// Gets the status of the message.
    /// </summary>
    [JsonConverter(typeof(SafeJsonStringEnumMemberConverter<MessageStatus>))]
    public enum MessageStatus
    {
        /// <summary>
        /// Default value.
        /// </summary>
        [JsonPropertyName(DefaultValues.Models.DefaultEnumValueName)]
        Unknown = 0,

        /// <summary>
        /// Have to be confidential.
        /// </summary>
        [JsonPropertyName("definitief")]
        Definitive = 1
    }
}