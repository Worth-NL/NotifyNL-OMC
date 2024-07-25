// © 2023, Worth Systems.

using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Helpers;
using EventsHandler.Mapping.Models.Interfaces;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.NotificatieApi
{
    /// <summary>
    /// The dynamic attributes of the notification retrieved from "OpenNotificaties" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct EventAttributes : IJsonSerializable
    {
        #region Metadata
        private static readonly ConcurrentDictionary<Channels, PropertiesMetadata> s_properties = new();

        /// <summary>
        /// Gets metadata of all public instance properties from the <see cref="EventAttributes"/> POCO model.
        /// </summary>
        internal readonly PropertiesMetadata Properties(Channels channel)
        {
            if (s_properties.IsEmpty)  // Metadata initialization
            {
                // Case
                s_properties.TryAdd(Channels.Cases, new PropertiesMetadata(this,
                    // Exclude objects
                    nameof(ObjectType),
                    // Exclude decisions
                    nameof(DecisionType), nameof(ResponsibleOrganization),
                    // Exclude orphans
                    nameof(this.Orphans)));

                // Object
                s_properties.TryAdd(Channels.Objects, new PropertiesMetadata(this,
                    // Exclude cases
                    nameof(CaseType), nameof(SourceOrganization), nameof(ConfidentialityNotice),
                    // Exclude decisions
                    nameof(DecisionType), nameof(ResponsibleOrganization),
                    // Exclude orphans
                    nameof(this.Orphans)));

                // Decisions
                s_properties.TryAdd(Channels.Decisions, new PropertiesMetadata(this,
                    // Exclude cases
                    nameof(CaseType), nameof(SourceOrganization), nameof(ConfidentialityNotice),
                    // Exclude objects
                    nameof(ObjectType),
                    // Exclude orphans
                    nameof(this.Orphans)));
            }

            return s_properties[channel];
        }
        #endregion

        #region Case properties
        /// <summary>
        /// Gets the URI of a case type.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("zaaktype")]
        [JsonPropertyOrder(0)]
        public Uri? CaseType { get; internal set; }

        /// <summary>
        /// Gets the name of the source organization.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("bronorganisatie")]
        [JsonPropertyOrder(1)]
        public string? SourceOrganization { get; internal set; }

        /// <inheritdoc cref="PrivacyNotices"/>
        [JsonInclude]
        [JsonPropertyName("vertrouwelijkheidaanduiding")]
        [JsonPropertyOrder(2)]
        public PrivacyNotices? ConfidentialityNotice { get; internal set; }
        #endregion

        #region Object properties
        /// <summary>
        /// Gets the URI of object type.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("objectType")]
        [JsonPropertyOrder(3)]
        public Uri? ObjectType { get; internal set; }
        #endregion

        #region Decision properties
        /// <summary>
        /// Gets the URI of decision type.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("besluittype")]
        [JsonPropertyOrder(4)]
        public Uri? DecisionType { get; internal set; }

        /// <summary>
        /// Gets the name of the responsible organization.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("verantwoordelijkeOrganisatie")]
        [JsonPropertyOrder(5)]
        public string? ResponsibleOrganization { get; internal set; }
        #endregion

        /// <summary>
        /// The JSON properties that couldn't be matched with properties of this specific POCO model => The orphans.
        /// </summary>
        [JsonInclude]
        [JsonExtensionData]      // Aggregate all JSON properties that couldn't be matched with this model
        [JsonPropertyOrder(99)]
        public Dictionary<string, object> Orphans { get; internal set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="EventAttributes"/> struct.
        /// </summary>
        public EventAttributes()
        {
        }

        #region Validation
        /// <summary>
        /// Checks whether the <see cref="EventAttributes"/> model wasn't initialized
        /// (and it has default values) for the <see cref="Channels.Cases"/> scenarios.
        /// </summary>
        /// <returns>
        ///   <see langword="true"/> if the <see cref="EventAttributes"/> isn't valid for <see cref="Channels.Cases"/>;
        ///   otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsInvalidCase(EventAttributes attributes)
        {
            // Properties required for cases scenarios
            return attributes.CaseType              == null || 
                   attributes.SourceOrganization    == null ||
                   attributes.ConfidentialityNotice == null;
        }

        /// <summary>
        /// Checks whether the <see cref="EventAttributes"/> model wasn't initialized
        /// (and it has default values) for the <see cref="Channels.Objects"/> scenarios.
        /// </summary>
        /// <returns>
        ///   <see langword="true"/> if the <see cref="EventAttributes"/> isn't valid for <see cref="Channels.Objects"/>;
        ///   otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsInvalidObject(EventAttributes attributes)
        {
            // Properties required for objects scenarios
            return attributes.ObjectType == null;
        }

        /// <summary>
        /// Checks whether the <see cref="EventAttributes"/> model wasn't initialized
        /// (and it has default values) for the <see cref="Channels.Decisions"/> scenarios.
        /// </summary>
        /// <returns>
        ///   <see langword="true"/> if the <see cref="EventAttributes"/> isn't valid for <see cref="Channels.Decisions"/>;
        ///   otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsInvalidDecision(EventAttributes attributes)
        {
            // Properties required for decisions scenarios
            return attributes.DecisionType            == null ||
                   attributes.ResponsibleOrganization == null;
        }
        #endregion
    }
}