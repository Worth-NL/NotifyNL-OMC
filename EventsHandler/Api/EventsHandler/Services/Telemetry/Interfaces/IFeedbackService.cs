// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Exceptions;

namespace EventsHandler.Services.Telemetry.Interfaces
{
    /// <summary>
    /// The service to collect and send feedback about the current business activities to the dedicated external API endpoint.
    /// </summary>
    public interface IFeedbackService
    {
        /// <summary>
        /// Reports to external API service that notification of type <see cref="NotifyMethods"/> was sent to "Notify NL" service.
        /// </summary>
        /// <param name="notification">The notification from "Notificatie API" Web service.</param>
        /// <param name="notificationMethod">The notification method.</param>
        /// <param name="message">The message to be passed along with the completion report.</param>
        /// <returns>
        ///   The callback URL prepared in response by "OpenKlant" web service.
        /// </returns>
        /// <exception cref="TelemetryException">The completion status could not be sent.</exception>
        internal Task<string> ReportCompletionAsync(NotificationEvent notification, NotifyMethods notificationMethod, string message);
    }
}