using Common.Settings.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ZgwModels.Mapping.Enums.NotificatieApi;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;

namespace ZgwModels.Mapping.Events
{
    /// <summary>
    /// Normalizes incoming payloads (CloudEvent or NotificationEvent) into a unified CloudEvent
    /// that can be forwarded to MijnOverheid.
    /// </summary>
    public class CloudEventNormalizer
    {
        private readonly OmcConfiguration _configuration;
        private readonly ILogger<CloudEventNormalizer> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudEventNormalizer"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration, used to retrieve the ZGW URN.</param>
        /// <param name="logger">The logger for recording normalization failures.</param>
        public CloudEventNormalizer(OmcConfiguration configuration, ILogger<CloudEventNormalizer> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Converts an incoming payload (CloudEvent or NotificationEvent) into a unified CloudEvent.
        /// </summary>
        /// <param name="payload">The raw JSON payload (JObject).</param>
        /// <returns>
        /// A <see cref="CloudEvent"/> if the payload is valid and can be mapped;
        /// otherwise <c>null</c> if the format is unsupported or required fields are missing.
        /// </returns>
        public CloudEvent? Normalize(JObject payload)
        {
            if (payload.ContainsKey("specversion"))
                return ConvertCloudEventJObject(payload);

            if (!payload.ContainsKey("kanaal"))
            {
                _logger.LogWarning("Payload does not contain 'kanaal' – skipping normalization.");
                return null;
            }

            NotificationEvent notification = payload.ToObject<NotificationEvent>();
            if (notification.IsInvalidEvent(out _))
            {
                _logger.LogWarning("NotificationEvent deserialization failed or is invalid.");
                return null;
            }

            return ConvertNotification(notification);
        }

        /// <summary>
        /// Converts a CloudEvent JObject into a <see cref="CloudEvent"/>.
        /// </summary>
        /// <param name="payload">The incoming CloudEvent JSON.</param>
        /// <returns>
        /// A <see cref="CloudEvent"/> if all required fields are present and non‑empty;
        /// otherwise <c>null</c>.
        /// </returns>
        private CloudEvent? ConvertCloudEventJObject(JObject payload)
        {
            // Required fields: specversion, type, source, subject, id, time
            string? specVersion = payload.Value<string>("specversion");
            if (string.IsNullOrWhiteSpace(specVersion))
            {
                _logger.LogWarning("CloudEvent missing 'specversion'.");
                return null;
            }

            string? type = payload.Value<string>("type");
            if (string.IsNullOrWhiteSpace(type))
            {
                _logger.LogWarning("CloudEvent missing 'type'.");
                return null;
            }

            string? source = payload.Value<string>("source");
            if (string.IsNullOrWhiteSpace(source))
            {
                _logger.LogWarning("CloudEvent missing 'source'.");
                return null;
            }

            string? subject = payload.Value<string>("subject");
            if (string.IsNullOrWhiteSpace(subject))
            {
                _logger.LogWarning("CloudEvent missing 'subject'.");
                return null;
            }

            string? id = payload.Value<string>("id");
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("CloudEvent missing 'id'.");
                return null;
            }

            DateTime? time = payload.Value<DateTime?>("time");
            if (!time.HasValue)
            {
                _logger.LogWarning("CloudEvent missing 'time'.");
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
        /// <param name="notification">The incoming NotificationEvent.</param>
        /// <returns>
        /// A <see cref="CloudEvent"/> if the event type is known and all required data is present;
        /// otherwise <c>null</c>.
        /// </returns>
        private CloudEvent? ConvertNotification(NotificationEvent notification)
        {
            // Try to map to a known CloudEvent type – returns null if unknown
            string? eventType = MapToCloudEventType(notification);
            if (eventType == null)
            {
                _logger.LogWarning("Unsupported NotificationEvent mapping: Channel={Channel}, Resource={Resource}, Action={Action}",
                    notification.Channel, notification.Resource, notification.Action);
                return null; // unsupported mapping
            }

            // Validate MainObjectUri
            Uri caseUri = notification.MainObjectUri;
            if (string.IsNullOrWhiteSpace(caseUri.ToString()))
            {
                _logger.LogWarning("NotificationEvent has empty MainObjectUri.");
                return null;
            }

            // Extract UUID (last segment)
            string caseUuid = caseUri.Segments.Last();
            if (string.IsNullOrWhiteSpace(caseUuid))
            {
                _logger.LogWarning("Could not extract UUID from MainObjectUri: {MainObjectUri}", caseUri);
                return null;
            }

            // ResourceUri is required for DataRef
            Uri resourceUri = notification.ResourceUri;
            if (string.IsNullOrWhiteSpace(resourceUri.ToString()))
            {
                _logger.LogWarning("NotificationEvent has empty ResourceUri.");
                return null;
            }

            // Validate that the ZGW URN is configured
            string source = _configuration.ZGW.Urn();
            if (string.IsNullOrWhiteSpace(source))
            {
                _logger.LogWarning("ZGW URN is not configured; cannot set CloudEvent Source.");
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
                DataRef = resourceUri.ToString(),
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
                _ => null // unknown – will skip forwarding
            };
        }
    }
}