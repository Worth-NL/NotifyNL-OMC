// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Enums.OpenZaak;
using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak.Decision
{
    /// <summary>
    /// The information object related to the <see cref="DecisionResource"/> retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct InfoObject : IJsonSerializable
    {
        /// <inheritdoc cref="PrivacyNotices"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("vertrouwelijkheidaanduiding")]
        [JsonPropertyOrder(0)]
        public PrivacyNotices Confidentiality { get; internal set; }  // TODO: Test options for this enum. Only "openbaar" is expected and this enum is reused from notification

        /// <inheritdoc cref="MessageStatus"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("status")]
        [JsonPropertyOrder(1)]
        public MessageStatus Status { get; internal set; }

        /// <summary>
        /// The type of the <see cref="InfoObject"/> in <see cref="Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("informatieobjecttype")]
        [JsonPropertyOrder(2)]
        public Uri TypeUri { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoObject"/> struct.
        /// </summary>
        public InfoObject()
        {
        }
    }
}