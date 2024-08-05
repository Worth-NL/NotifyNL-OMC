// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Enums.OpenZaak;
using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak
{
    /// <summary>
    /// The information object related to the <see cref="Decision"/> retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable" />
    public struct InfoObject : IJsonSerializable
    {
        /// <inheritdoc cref="PrivacyNotices"/>
        [JsonInclude]
        [JsonPropertyName("vertrouwelijkheidaanduiding")]
        [JsonPropertyOrder(0)]
        public PrivacyNotices Confidentiality { get; internal set; }  // TODO: Test options for this enum. Only "openbaar" is expected and this enum is reused from notification

        /// <inheritdoc cref="MessageStatus"/>
        [JsonInclude]
        [JsonPropertyName("status")]
        [JsonPropertyOrder(1)]
        public MessageStatus Status { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoObject"/> struct.
        /// </summary>
        public InfoObject()
        {
        }
    }
}