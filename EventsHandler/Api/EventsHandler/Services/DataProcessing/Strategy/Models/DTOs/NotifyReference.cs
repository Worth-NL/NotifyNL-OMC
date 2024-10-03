// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using System.Text.Json.Serialization;

namespace EventsHandler.Services.DataProcessing.Strategy.Models.DTOs
{
    /// <summary>
    /// The set of data used to pass as a "reference" to "Notify NL" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable" />
    public struct NotifyReference : IJsonSerializable  // NOTE: This model is used in endpoints + Swagger UI examples and it must be public
    {
        /// <inheritdoc cref="NotificationEvent"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(0)]
        public NotificationEvent Notification { get; internal set; } = default;

        /// <summary>
        /// The extracted GUID component from <see cref="Case.Uri"/>.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(1)]
        public Guid? CaseId { get; internal set; } = Guid.Empty;  // NOTE: Sometimes, case URI might be missing

        /// <summary>
        /// The extracted GUID component from <see cref="CommonPartyData.Uri"/>.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(2)]
        public Guid PartyId { get; internal set; } = Guid.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyReference"/> struct.
        /// </summary>
        public NotifyReference()
        {
        }
    }
}