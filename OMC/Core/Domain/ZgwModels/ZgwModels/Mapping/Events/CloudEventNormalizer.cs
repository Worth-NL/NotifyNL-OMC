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
        private readonly OmcConfiguration _configuration;
        private readonly ILogger<CloudEventNormalizer> _logger;
        private readonly ISerializationService _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudEventNormalizer"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration, used to retrieve the ZGW URN.</param>
        /// <param name="logger">The logger for recording normalization failures.</param>
        /// <param name="serializer">The serializer service used to deserialize NotificationEvents (same as the rest of the project).</param>
        public CloudEventNormalizer(
            OmcConfiguration configuration,
            ILogger<CloudEventNormalizer> logger,
            ISerializationService serializer)
        {
            _configuration = configuration;
            _logger = logger;
            _serializer = serializer;
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

            if (payload.ContainsKey("kanaal") || payload.ContainsKey("Kanaal"))
            {
                // Use the SAME proven serializer as the Listen endpoint
                // This respects [EnumMember] attributes and all custom converters.
                string json = payload.ToString();
                NotificationEvent notification = _serializer.Deserialize<NotificationEvent>(json);

                if (notification.IsInvalidEvent(out _))
                {
                    _logger.LogWarning("NotificationEvent deserialization produced invalid data.");
                    return null;
                }

                return ConvertNotification(notification);
            }

            _logger.LogWarning("Payload format not recognized (missing specversion or kanaal).");
            return null;
        }

        /// <summary>
        /// Converts a CloudEvent JObject into a <see cref="CloudEvent"/>.
        /// </summary>
        private CloudEvent? ConvertCloudEventJObject(JObject payload)
        {
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
        /// Converts a deserialized <see cref="NotificationEvent"/> into a <see cref="CloudEvent"/>.
        /// </summary>
        private CloudEvent? ConvertNotification(NotificationEvent notification)
        {
            string? eventType = MapToCloudEventType(notification);
            if (eventType == null)
            {
                _logger.LogWarning("Unsupported mapping for Channel={Channel}, Resource={Resource}, Action={Action}",
                    notification.Channel, notification.Resource, notification.Action);
                return null;
            }

            Uri caseUri = notification.MainObjectUri;
            if (string.IsNullOrWhiteSpace(caseUri.ToString()))
            {
                _logger.LogWarning("MainObjectUri is empty.");
                return null;
            }

            string caseUuid = caseUri.Segments.Last();
            if (string.IsNullOrWhiteSpace(caseUuid))
            {
                _logger.LogWarning("Could not extract UUID from MainObjectUri: {MainObjectUri}", caseUri);
                return null;
            }

            Uri resourceUri = notification.ResourceUri;
            if (string.IsNullOrWhiteSpace(resourceUri.ToString()))
            {
                _logger.LogWarning("ResourceUri is empty.");
                return null;
            }

            string source = _configuration.ZGW.Urn();
            if (string.IsNullOrWhiteSpace(source))
            {
                _logger.LogWarning("ZGW URN is not configured.");
                return null;
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

        private static string? MapToCloudEventType(NotificationEvent notification)
        {
            return (notification.Channel, notification.Resource, notification.Action) switch
            {
                (Channels.Cases, Resources.Status, Actions.Create) => "nl.overheid.zaken.zaak-gemuteerd",
                _ => null
            };
        }
    }
}