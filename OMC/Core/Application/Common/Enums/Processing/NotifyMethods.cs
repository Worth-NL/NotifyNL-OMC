// © 2023, Worth Systems.

using Common.Constants;
using System.Text.Json.Serialization;

namespace Common.Enums.Processing
{
    /// <summary>
    /// The notification method used by "Notify NL" API Client to communicate with a citizen.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]  // NOTE: Simple JSON converter to display enum options in Swagger UI
    public enum NotifyMethods
    {
        /// <summary>
        /// Communication method: None => "do not notify me".
        /// </summary>
        [JsonPropertyName(CommonValues.Default.Models.DefaultEnumValueName)]
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
        Sms = 3
    }
}