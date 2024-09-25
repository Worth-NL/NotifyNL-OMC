// © 2024, Worth Systems.

using EventsHandler.Constants;
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

        /// <inheritdoc cref="Case.Uri"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(1)]
        public Uri? CaseUri { get; internal set; } = DefaultValues.Models.EmptyUri;  // NOTE: Sometimes, case URI might be missing

        /// <inheritdoc cref="CommonPartyData.Uri"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyOrder(2)]
        public Uri PartyUri { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyReference"/> struct.
        /// </summary>
        public NotifyReference()
        {
        }
    }
}