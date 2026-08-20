using System.Text.Json.Serialization;

namespace WebQueries.MOBB.Models
{
    public readonly struct MessageBoxRequest
    {
        [JsonPropertyName("bsn")]
        public string Bsn { get; init; }

        [JsonPropertyName("message")]
        public string Message { get; init; }

        [JsonPropertyName("reference")]
        public string Reference { get; init; }

        [JsonPropertyName("personalisation")]
        public Personalisation? Personalisation { get; init; }

        [JsonPropertyName("attachments")]
        public List<MobbAttachment>? Attachments { get; init; }

        [JsonPropertyName("batchId")]
        public string? BatchId { get; init; }

        [JsonPropertyName("messageType")]
        public string? MessageType { get; init; }
    }

    public readonly struct Personalisation
    {
        [JsonPropertyName("subject")]
        public string? Subject { get; init; }
    }

    public readonly struct MobbAttachment
    {
        [JsonPropertyName("filename")]
        public string? Filename { get; init; }

        [JsonPropertyName("content")]
        public string? Content { get; init; }
    }
}