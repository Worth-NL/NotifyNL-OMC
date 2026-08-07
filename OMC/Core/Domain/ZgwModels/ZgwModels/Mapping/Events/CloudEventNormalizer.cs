using Common.Settings.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ZgwModels.Mapping.Enums.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;
using ZgwModels.Serialization.Interfaces;

namespace ZgwModels.Mapping.Events
{
    /// <summary>
    /// Normalizes incoming payloads (CloudEvent or NotificationEvent) into a unified CloudEvent
    /// that can be forwarded to MijnOverheid.
    /// </summary>
    public class CloudEventNormalizer
    {
        private const string ExpectedNotificationFormats =
            """
            {"actie":"create","kanaal":"zaken","resource":"status","kenmerken":{"zaaktype":"<zaaktype-url>","bronorganisatie":"<rsin>","vertrouwelijkheidaanduiding":"openbaar"},"hoofdObject":"<zaak-url>","resourceUrl":"<status-url>","aanmaakdatum":"<ISO8601>"} (zaak-gemuteerd), or the same shape with "resource":"zaak" and "actie":"read" (zaak-geopend) / "actie":"destroy" (zaak-verwijderd)
            """;

        private readonly OmcConfiguration _configuration;
        private readonly ILogger<CloudEventNormalizer> _logger;
        private readonly ISerializationService _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudEventNormalizer"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration, used to retrieve the ZGW URN.</param>
        /// <param name="logger">The logger for recording normalization failures.</param>
        /// <param name="serializer">The input deserializing service, used to correctly map Dutch JSON keys onto <see cref="NotificationEvent"/>.</param>
        public CloudEventNormalizer(OmcConfiguration configuration, ILogger<CloudEventNormalizer> logger, ISerializationService serializer)
        {
            _configuration = configuration;
            _logger = logger;
            _serializer = serializer;
        }

        /// <summary>
        /// Converts an incoming payload (CloudEvent or NotificationEvent) into a unified CloudEvent.
        /// </summary>
        /// <param name="payload">The raw JSON payload (JObject).</param>
        /// <param name="reason">When the payload is rejected, describes specifically what was malformed or unsupported.</param>
        /// <returns>
        /// A <see cref="CloudEvent"/> if the payload is valid and can be mapped;
        /// otherwise <c>null</c> if the format is unsupported or required fields are missing.
        /// </returns>
        public CloudEvent? Normalize(JObject payload, out string? reason)
        {
            reason = null;

            if (payload.ContainsKey("specversion"))
                return ConvertCloudEventJObject(payload, out reason);

            if (!payload.ContainsKey("kanaal"))
            {
                reason = "Payload does not contain 'kanaal'.";
                _logger.LogWarning("{Reason}", reason);
                return null;
            }

            NotificationEvent notification = _serializer.Deserialize<NotificationEvent>(payload.ToString());
            if (notification.IsInvalidEvent(out _))
            {
                reason = "NotificationEvent deserialization failed or is invalid.";
                _logger.LogWarning("{Reason}", reason);
                return null;
            }

            return ConvertNotification(payload, notification, out reason);
        }

        /// <summary>
        /// Converts a CloudEvent JObject into a <see cref="CloudEvent"/>.
        /// </summary>
        /// <param name="payload">The incoming CloudEvent JSON.</param>
        /// <param name="reason">When the payload is rejected, describes specifically what was malformed or unsupported.</param>
        /// <returns>
        /// A <see cref="CloudEvent"/> if all required fields are present and non‑empty;
        /// otherwise <c>null</c>.
        /// </returns>
        private CloudEvent? ConvertCloudEventJObject(JObject payload, out string? reason)
        {
            reason = null;

            // Required fields: specversion, type, source, subject, id, time
            string? specVersion = payload.Value<string>("specversion");
            if (string.IsNullOrWhiteSpace(specVersion))
            {
                reason = $"CloudEvent is missing 'specversion'. Received: {payload}";
                _logger.LogWarning("{Reason}", reason);
                return null;
            }

            string? type = payload.Value<string>("type");
            if (string.IsNullOrWhiteSpace(type))
            {
                reason = $"CloudEvent is missing 'type'. Received: {payload}";
                _logger.LogWarning("{Reason}", reason);
                return null;
            }

            string? source = payload.Value<string>("source");
            if (string.IsNullOrWhiteSpace(source))
            {
                reason = $"CloudEvent is missing 'source'. Received: {payload}";
                _logger.LogWarning("{Reason}", reason);
                return null;
            }

            string? subject = payload.Value<string>("subject");
            if (string.IsNullOrWhiteSpace(subject))
            {
                reason = $"CloudEvent is missing 'subject'. Received: {payload}";
                _logger.LogWarning("{Reason}", reason);
                return null;
            }

            string? id = payload.Value<string>("id");
            if (string.IsNullOrWhiteSpace(id))
            {
                reason = $"CloudEvent is missing 'id'. Received: {payload}";
                _logger.LogWarning("{Reason}", reason);
                return null;
            }

            DateTime? time = payload.Value<DateTime?>("time");
            if (!time.HasValue)
            {
                reason = $"CloudEvent is missing 'time'. Received: {payload}";
                _logger.LogWarning("{Reason}", reason);
                return null;
            }

            return new CloudEvent
            {
                SpecVersion = specVersion,
                Type = type,
                Source = source,
                Subject = subject,
                Id = id,
                Time = time.Value,
                DataRef = payload.Value<string>("dataref"),
                DataContentType = payload.Value<string>("datacontenttype"),
                Data = null
            };
        }

        /// <summary>
        /// Converts a <see cref="NotificationEvent"/> into a <see cref="CloudEvent"/>.
        /// </summary>
        /// <param name="payload">The raw incoming JSON payload, used to echo back what was actually received when rejecting it.</param>
        /// <param name="notification">The incoming NotificationEvent.</param>
        /// <param name="reason">When the payload is rejected, describes specifically what was malformed or unsupported.</param>
        /// <returns>
        /// A <see cref="CloudEvent"/> if the event type is known and all required data is present;
        /// otherwise <c>null</c>.
        /// </returns>
        private CloudEvent? ConvertNotification(JObject payload, NotificationEvent notification, out string? reason)
        {
            reason = null;

            // Try to map to a known CloudEvent type – returns null if unknown
            string? eventType = MapToCloudEventType(notification);
            if (eventType == null)
            {
                reason = $"We were expecting a notification in a format such as: {ExpectedNotificationFormats}, but we received: {payload}";
                _logger.LogWarning("{Reason}", reason);
                return null; // unsupported mapping
            }

            // Validate MainObjectUri
            Uri caseUri = notification.MainObjectUri;
            if (string.IsNullOrWhiteSpace(caseUri.ToString()))
            {
                reason = $"NotificationEvent has an empty 'hoofdObject'. Received: {payload}";
                _logger.LogWarning("{Reason}", reason);
                return null;
            }

            // Extract UUID (last segment)
            string caseUuid = caseUri.Segments.Last();
            if (string.IsNullOrWhiteSpace(caseUuid))
            {
                reason = $"Could not extract a UUID from 'hoofdObject' ({caseUri}). Received: {payload}";
                _logger.LogWarning("{Reason}", reason);
                return null;
            }

            // ResourceUri is required for DataRef
            Uri resourceUri = notification.ResourceUri;
            if (string.IsNullOrWhiteSpace(resourceUri.ToString()))
            {
                reason = $"NotificationEvent has an empty 'resourceUrl'. Received: {payload}";
                _logger.LogWarning("{Reason}", reason);
                return null;
            }

            // Validate that the ZGW URN is configured
            string source = _configuration.ZGW.Urn();
            if (string.IsNullOrWhiteSpace(source))
            {
                reason = "ZGW URN is not configured on this OMC instance; cannot set CloudEvent Source.";
                _logger.LogWarning("{Reason}", reason);
                return null; // missing source configuration
            }

            return new CloudEvent
            {
                SpecVersion = "1.0",
                Type = eventType,
                Source = source,
                Subject = caseUuid,
                Id = Guid.NewGuid().ToString(),
                Time = DateTime.UtcNow,
                DataRef = caseUri.AbsolutePath,
                DataContentType = "application/json",
                Data = null
            };
        }

        /// <summary>
        /// Strictly maps a <see cref="NotificationEvent"/> to a CloudEvent type string.
        /// </summary>
        /// <param name="notification">The NotificationEvent to map.</param>
        /// <returns>
        /// The CloudEvent type (e.g., "nl.overheid.zaken.zaak-gemuteerd") if the combination
        /// of <see cref="NotificationEvent.Channel"/>, <see cref="NotificationEvent.Resource"/>,
        /// and <see cref="NotificationEvent.Action"/> is known; otherwise <c>null</c>.
        /// </returns>
        private static string? MapToCloudEventType(NotificationEvent notification)
        {
            return (notification.Channel, notification.Resource, notification.Action) switch
            {
                (Channels.Cases, Resources.Status, Actions.Create) => "nl.overheid.zaken.zaak-gemuteerd",
                (Channels.Cases, Resources.Case, Actions.Read) => "nl.overheid.zaken.zaak-geopend",
                (Channels.Cases, Resources.Case, Actions.Destroy) => "nl.overheid.zaken.zaak-verwijderd",
                _ => null // unknown – will skip forwarding
            };
        }
    }
}