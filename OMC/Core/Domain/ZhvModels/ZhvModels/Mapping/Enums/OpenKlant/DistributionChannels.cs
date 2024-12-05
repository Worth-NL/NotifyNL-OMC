// © 2023, Worth Systems.

using Common.Constants;
using Common.Enums.Converters;
using System.Text.Json.Serialization;

namespace ZhvModels.Mapping.Enums.OpenKlant
{
    /// <summary>
    /// The distribution channel. The method how the recipient (e.g., citizen or organization) wants to be notified.
    /// </summary>
    [JsonConverter(typeof(SafeJsonStringEnumMemberConverter<DistributionChannels>))]
    public enum DistributionChannels
    {
        /// <summary>
        /// The default value.
        /// </summary>
        [JsonPropertyName(CommonValues.Default.Models.DefaultEnumValueName)]
        Unknown = 0,

        /// <summary>
        /// Communication method: None => "do not notify me".
        /// </summary>
        [JsonPropertyName("geen")]
        None = 1,

        /// <summary>
        /// Communication method: e-mail.
        /// </summary>
        [JsonPropertyName("email")]
        Email = 2,

        /// <summary>
        /// Communication method: SMS.
        /// </summary>
        [JsonPropertyName("sms")]
        Sms = 3,

        /// <summary>
        /// Communication method: SMS and e-mail.
        /// </summary>
        [JsonPropertyName("beiden")]
        Both = 4
    }
}