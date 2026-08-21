using System.Text.Json.Serialization;

namespace WebQueries.MOBB.Models
{
    /// <summary>
    /// The request body sent to the MOBB (MijnOverheid Berichtenbox) Notify API endpoint.
    /// </summary>
    public readonly struct MessageBoxRequest
    {
        /// <summary>
        /// The recipient's BSN (Citizen Service Number).
        /// </summary>
        [JsonPropertyName("bsn")]
        public string Bsn { get; init; }

        /// <summary>
        /// The message text to be delivered.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; init; }

        /// <summary>
        /// The compressed &amp; Base64-encoded reference round-tripped through Notify NL, used to
        /// correlate a later delivery-receipt callback back to the originating message.
        /// </summary>
        [JsonPropertyName("reference")]
        public string Reference { get; init; }

        /// <summary>
        /// Additional personalization data for the message.
        /// </summary>
        [JsonPropertyName("personalisation")]
        public Personalisation? Personalisation { get; init; }

        /// <summary>
        /// The attachments (bijlagen) to include with the message, if any.
        /// </summary>
        [JsonPropertyName("attachments")]
        public List<MobbAttachment>? Attachments { get; init; }

        /// <summary>
        /// The batch identifier grouping this message with others sent together, if applicable.
        /// </summary>
        [JsonPropertyName("batchId")]
        public string? BatchId { get; init; }

        /// <summary>
        /// The type of the message, used for whitelist validation.
        /// </summary>
        [JsonPropertyName("messageType")]
        public string? MessageType { get; init; }
    }

    /// <summary>
    /// Personalization data included in a <see cref="MessageBoxRequest"/>.
    /// </summary>
    public readonly struct Personalisation
    {
        /// <summary>
        /// The subject of the message.
        /// </summary>
        [JsonPropertyName("subject")]
        public string? Subject { get; init; }
    }

    /// <summary>
    /// An attachment included in a <see cref="MessageBoxRequest"/>.
    /// </summary>
    public readonly struct MobbAttachment
    {
        /// <summary>
        /// The filename of the attachment.
        /// </summary>
        [JsonPropertyName("filename")]
        public string? Filename { get; init; }

        /// <summary>
        /// The Base64-encoded content of the attachment.
        /// </summary>
        [JsonPropertyName("content")]
        public string? Content { get; init; }
    }
}
