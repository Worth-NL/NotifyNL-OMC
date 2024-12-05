// © 2024, Worth Systems.

using Common.Constants;
using Common.Enums.Converters;
using System.Text.Json.Serialization;

namespace ZhvModels.Mapping.Enums.OpenZaak
{
    /// <summary>
    /// The status of the message.
    /// </summary>
    [JsonConverter(typeof(SafeJsonStringEnumMemberConverter<MessageStatus>))]
    public enum MessageStatus
    {
        /// <summary>
        /// The default value.
        /// </summary>
        [JsonPropertyName(CommonValues.Default.Models.DefaultEnumValueName)]
        Unknown = 0,

        /// <summary>
        /// The status is definitive / complete.
        /// </summary>
        [JsonPropertyName("definitief")]
        Definitive = 1

        // TODO: What is option other than "definitief"?
    }
}