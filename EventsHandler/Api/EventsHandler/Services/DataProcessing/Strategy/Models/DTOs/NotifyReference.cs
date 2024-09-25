// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;

namespace EventsHandler.Services.DataProcessing.Strategy.Models.DTOs
{
    /// <summary>
    /// The set of data used to pass as a "reference" to "Notify NL" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable" />
    internal readonly struct NotifyReference : IJsonSerializable
    {
        /// <inheritdoc cref="NotificationEvent"/>
        internal NotificationEvent Notification { get; }

        /// <inheritdoc cref="Case.Uri"/>
        internal Uri? CaseUri { get; } = DefaultValues.Models.EmptyUri;

        /// <inheritdoc cref="CommonPartyData.Uri"/>
        internal Uri PartyUri { get; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyReference"/> struct.
        /// </summary>
        internal NotifyReference(NotificationEvent notification, Uri? caseUri, Uri partyUri)
        {
            this.Notification = notification;
            this.CaseUri = caseUri;
            this.PartyUri = partyUri;
        }
    }
}