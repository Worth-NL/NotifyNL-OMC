using System.Text.Json.Serialization;

namespace WebQueries.MijnOverheid.Models
{
    /// <summary>
    /// Represents a CloudEvent according to the NL‑GOV profile, used to notify MijnOverheid about
    /// case‑related events (zaak‑gemuteerd, zaak‑verwijderd, zaak‑geopend).
    /// </summary>
    public record CloudEvent
    {
        /// <summary>
        /// Version of the CloudEvents specification. Always "1.0".
        /// </summary>
        [JsonPropertyName("specversion")]
        public string SpecVersion { get; set; } = "1.0";

        /// <summary>
        /// Type of the event. One of:
        /// - "nl.overheid.zaken.zaak-gemuteerd"
        /// - "nl.overheid.zaken.zaak-verwijderd"
        /// - "nl.overheid.zaken.zaak-geopend"
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Identifies the organisation responsible for the case, formatted as a URN.
        /// Allowed formats: urn:nld:oin:{OIN}:{applicatie}, urn:nld:kvk:{KVK}:{applicatie},
        /// or urn:nld:rsin:{RSIN}:{applicatie}.
        /// </summary>
        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// The unique identifier (UUID) of the case.
        /// </summary>
        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Unique identifier (UUID) of this specific event. Used for deduplication.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp of when the event occurred, in UTC (RFC 3339 format).
        /// </summary>
        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// Relative URL pointing to the REST API endpoint where the full case details can be retrieved.
        /// Combined with the base URL of the source system to form the complete endpoint.
        /// </summary>
        [JsonPropertyName("dataref")]
        public string DataRef { get; set; } = string.Empty;

        /// <summary>
        /// Media type of the data field. Always "application/json" for these events.
        /// </summary>
        [JsonPropertyName("datacontenttype")]
        public string DataContentType { get; set; } = string.Empty;

        /// <summary>
        /// Event‑specific payload. For "zaak‑gemuteerd" this contains the "externAttenderen" flag.
        /// For "zaak‑verwijderd" and "zaak‑geopend" this is null.
        /// </summary>
        [JsonPropertyName("data")]
        public Object? Data { get; set; }

        /// <summary>
        /// Holds the data payload for the CloudEvent.
        /// </summary>
        public record DataObject
        {
            /// <summary>
            /// Indicates whether MijnOverheid should send an email notification to the citizen
            /// (only applicable to "zaak‑gemuteerd" events). If true, MijnOverheid will attempt
            /// to notify the citizen, subject to their notification preferences.
            /// </summary>
            [JsonPropertyName("externAttenderen")]
            public bool? ExternAttenderen { get; set; }
        }
    }
}