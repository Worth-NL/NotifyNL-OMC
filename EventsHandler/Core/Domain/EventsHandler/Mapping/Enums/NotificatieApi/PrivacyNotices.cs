// © 2023, Worth Systems.

using Common.Constants;
using EventsHandler.Mapping.Converters;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Enums.NotificatieApi
{
    /// <summary>
    /// The confidentiality notice of the current notification.
    /// </summary>
    [JsonConverter(typeof(SafeJsonStringEnumMemberConverter<PrivacyNotices>))]
    public enum PrivacyNotices
    {
        /// <summary>
        /// The default value.
        /// </summary>
        [JsonPropertyName(DefaultValues.Models.DefaultEnumValueName)]
        Unknown = 0,

        /// <summary>
        /// The confidential notice => private.
        /// </summary>
        [JsonPropertyName("vertrouwelijk")]
        Confidential = 1,

        /// <summary>
        /// The non-confidential notice => public.
        /// </summary>
        [JsonPropertyName("openbaar")]
        NonConfidential = 2
    }
}