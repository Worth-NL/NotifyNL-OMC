namespace ZgwModels.Mapping.Events
{
    /// <summary>
    /// Incoming CloudEvent payload from "ZGW API" events queue, which can be forwarded to the "Notify NL" API service.
    /// </summary>
    public readonly struct CloudEvent
    {
        /// <summary>
        /// Specification version of the CloudEvent. Currently, only "1.0" is supported.
        /// </summary>
        public string SpecVersion { get; init; } = "1.0";
        /// <summary>
        /// Event type, which indicates the nature of the event. For example, "nl.overheid.zaken.zaak-gemuteerd" for a case mutation event.
        /// </summary>
        public string Type { get; init; } = string.Empty;
        /// <summary>
        /// Urn identifying the source of the event, typically representing the organization responsible for the case. For example, "urn:nld:oin:{OIN}:{applicatie}".
        /// </summary>
        public string Source { get; init; } = string.Empty;
        /// <summary>
        /// The unique identifier (UUID) of the case associated with this event. This is used to correlate events with specific cases in the system.
        /// </summary>
        public string Subject { get; init; } = string.Empty;
        /// <summary>
        /// The unique identifier (UUID) of this specific event instance. This is used for deduplication and tracking of events.
        /// </summary>
        public string Id { get; init; } = string.Empty;
        /// <summary>
        /// Timestamp indicating when the event occurred, in UTC. This is used to determine the order of events and for auditing purposes.
        /// </summary>
        public DateTime Time { get; init; } = DateTime.UtcNow;
        /// <summary>
        /// The relative URL pointing to the REST API endpoint where the full case details can be retrieved. This is combined with the base URL of the source system to form the complete endpoint.
        /// </summary>
        public string? DataRef { get; init; }
        /// <summary>
        /// Data content type of the event payload. For example, "application/json" indicates that the data field contains JSON-formatted data.
        /// </summary>
        public string? DataContentType { get; init; }
        /// <summary>
        /// Data payload of the event, which may contain event-specific information. For example, for a "zaak-gemuteerd" event, this may include the "externAttenderen" flag. For other event types, this may be null.
        /// </summary>
        public object? Data { get; init; }

        /// <summary>
        /// Constructs a new instance of the <see cref="CloudEvent"/> struct.
        /// </summary>
        public CloudEvent() { }
    }
}
