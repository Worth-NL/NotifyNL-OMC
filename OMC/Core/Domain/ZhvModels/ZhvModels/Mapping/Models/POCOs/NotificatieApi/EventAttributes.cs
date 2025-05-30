﻿// © 2023, Worth Systems.

using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using ZhvModels.Mapping.Enums.NotificatieApi;
using ZhvModels.Mapping.Helpers;
using ZhvModels.Mapping.Models.Interfaces;
using ZhvModels.Mapping.Models.POCOs.Objecten.Message;
using ZhvModels.Mapping.Models.POCOs.Objecten.Task;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;
using ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision;

namespace ZhvModels.Mapping.Models.POCOs.NotificatieApi
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
        public readonly PropertiesMetadata Properties(Channels channel)
        {
            if (s_properties.IsEmpty)  // Metadata initialization
            {
                // Case
                s_properties.TryAdd(Channels.Cases, new PropertiesMetadata(this,
                    // Exclude objects
                    nameof(ObjectTypeUri),
                    // Exclude decisions
                    nameof(DecisionTypeUri), nameof(ResponsibleOrganization),
                    // Exclude orphans
                    nameof(this.Orphans)));

                // Object
                s_properties.TryAdd(Channels.Objects, new PropertiesMetadata(this,
                    // Exclude cases
                    nameof(CaseTypeUri), nameof(SourceOrganization), nameof(ConfidentialityNotice),
                    // Exclude decisions
                    nameof(DecisionTypeUri), nameof(ResponsibleOrganization),
                    // Exclude orphans
                    nameof(this.Orphans)));

                // Decisions
                s_properties.TryAdd(Channels.Decisions, new PropertiesMetadata(this,
                    // Exclude cases
                    nameof(CaseTypeUri), nameof(SourceOrganization), nameof(ConfidentialityNotice),
                    // Exclude objects
                    nameof(ObjectTypeUri),
                    // Exclude orphans
                    nameof(this.Orphans)));
            }

            return s_properties[channel];
        }
        #endregion

        #region Case properties
        /// <summary>
        /// The <see cref="Case"/> type in <see cref="Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonPropertyName("zaaktype")]
        [JsonPropertyOrder(0)]
        public Uri? CaseTypeUri { get; init; }

        /// <summary>
        /// The name of the source organization.
        /// </summary>
        [JsonPropertyName("bronorganisatie")]
        [JsonPropertyOrder(1)]
        public string? SourceOrganization { get; set; }

        /// <inheritdoc cref="PrivacyNotices"/>
        [JsonPropertyName("vertrouwelijkheidaanduiding")]
        [JsonPropertyOrder(2)]
        public PrivacyNotices? ConfidentialityNotice { get; init; }
        #endregion

        #region Object properties
        /// <summary>
        /// The <see cref="CommonTaskData"/> from Task or <see cref="MessageObject"/> type in <see cref="Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonPropertyName("objectType")]
        [JsonPropertyOrder(3)]
        public Uri? ObjectTypeUri { get; init; }
        #endregion

        #region Decision properties
        /// <summary>
        /// The <see cref="Decision"/> type in <see cref="Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonPropertyName("besluittype")]
        [JsonPropertyOrder(4)]
        public Uri? DecisionTypeUri { get; init; }

        /// <summary>
        /// The name of the responsible organization.
        /// </summary>
        [JsonPropertyName("verantwoordelijkeOrganisatie")]
        [JsonPropertyOrder(5)]
        public string? ResponsibleOrganization { get; init; }
        #endregion

        /// <summary>
        /// The JSON properties that couldn't be matched with properties of this specific POCO model => The orphans.
        /// </summary>
        [JsonExtensionData]  // Aggregate all JSON properties that couldn't be matched with this model
        [JsonPropertyOrder(99)]
        public Dictionary<string, object> Orphans { get; set; } = [];

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
        public static bool IsInvalidCase(EventAttributes attributes)
        {
            // Properties required for cases scenarios
            return attributes.CaseTypeUri           == null || 
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
        public static bool IsInvalidObject(EventAttributes attributes)
        {
            // Properties required for objects scenarios
            return attributes.ObjectTypeUri == null;
        }

        /// <summary>
        /// Checks whether the <see cref="EventAttributes"/> model wasn't initialized
        /// (and it has default values) for the <see cref="Channels.Decisions"/> scenarios.
        /// </summary>
        /// <returns>
        ///   <see langword="true"/> if the <see cref="EventAttributes"/> isn't valid for <see cref="Channels.Decisions"/>;
        ///   otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsInvalidDecision(EventAttributes attributes)
        {
            // Properties required for decisions scenarios
            return attributes.DecisionTypeUri         == null ||
                   attributes.ResponsibleOrganization == null;
        }
        #endregion
    }
}