// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Helpers;
using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Constants;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi
{
    /// <summary>
    /// The dynamic attributes of the notification retrieved from "Notificatie API" Web service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct EventAttributes : IJsonSerializable
    {
        #region Metadata
        private static readonly object s_lock = new();
        private static PropertiesMetadata? s_properties;

        /// <summary>
        /// Gets metadata of all public instance properties from the <see cref="EventAttributes"/> POCO model.
        /// </summary>
        internal readonly PropertiesMetadata Properties
        {
            get
            {
                if (s_properties == null)
                {
                    // Critical Section
                    lock (s_lock)
                    {
                        s_properties ??= new PropertiesMetadata(this, nameof(this.Orphans));
                    }
                }

                return s_properties;
            }
        }
        #endregion

        /// <summary>
        /// Gets the URI to "OpenKlant" Web service.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("objectType")]
        [JsonPropertyOrder(0)]
        public Uri? ObjectType { get; internal set; }

        /// <summary>
        /// Gets the URI to "OpenZaak" Web service.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("zaaktype")]
        [JsonPropertyOrder(1)]
        public Uri? CaseType { get; internal set; }

        /// <summary>
        /// Gets the name of the source organization.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("bronorganisatie")]
        [JsonPropertyOrder(2)]
        public string? SourceOrganization { get; internal set; }

        /// <inheritdoc cref="PrivacyNotices"/>
        [JsonInclude]
        [JsonPropertyName("vertrouwelijkheidaanduiding")]
        [JsonPropertyOrder(3)]
        public PrivacyNotices? ConfidentialityNotice { get; internal set; }

        /// <summary>
        /// The JSON properties that couldn't be matched with properties of this specific POCO model => The orphans.
        /// </summary>
        [JsonInclude]
        [JsonExtensionData]     // Aggregate all JSON properties that couldn't be matched with this model
        [JsonPropertyOrder(4)]
        public Dictionary<string, object> Orphans { get; internal set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="EventAttributes"/> struct.
        /// </summary>
        public EventAttributes()
        {
        }

        /// <summary>
        /// Checks whether the <see cref="EventAttributes"/> model wasn't initialized (and it has default values).
        /// </summary>
        internal static bool IsDefault(EventAttributes attributes)
        {
            return attributes.ObjectType == null &&
                   attributes.CaseType   == null &&
                   attributes is
                   {
                       SourceOrganization: null,
                       ConfidentialityNotice: null
                   };
        }
    }
}