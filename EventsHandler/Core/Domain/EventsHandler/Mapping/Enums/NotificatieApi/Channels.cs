// © 2023, Worth Systems.

using Common.Constants;
using EventsHandler.Mapping.Converters;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Enums.NotificatieApi
{
    /// <summary>
    /// The name of the channel to which the post should be published.
    /// </summary>
    [JsonConverter(typeof(SafeJsonStringEnumMemberConverter<Channels>))]
    public enum Channels
    {
        /// <summary>
        /// The default value.
        /// </summary>
        [JsonPropertyName(CommonValues.Default.Models.DefaultEnumValueName)]
        Unknown = 0,

        /// <summary>
        /// Cases channel.
        /// </summary>
        [JsonPropertyName("zaken")]
        Cases = 1,

        /// <summary>
        /// Objects channel.
        /// </summary>
        [JsonPropertyName("objecten")]
        Objects = 2,

        /// <summary>
        /// Decisions channel.
        /// </summary>
        [JsonPropertyName("besluiten")]
        Decisions = 3
    }
}