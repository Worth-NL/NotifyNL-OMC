// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Versioning;
using EventsHandler.Exceptions;

namespace EventsHandler.Services.Telemetry.Interfaces
{
    /// <summary>
    /// The service to collect and send feedback about the current business activities to the dedicated external API endpoint.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    public interface ITelemetryService : IVersionDetails
    {
        /// <summary>
        /// Reports to external API service that notification of type <see cref="NotifyMethods"/> was sent to "Notify NL" service.
        /// </summary>
        /// <returns>
        ///   The JSON response from an external Telemetry Web API service.
        /// </returns>
        /// <exception cref="TelemetryException">The completion status could not be sent.</exception>
        internal Task<string> ReportCompletionAsync(NotificationEvent notification, NotifyMethods notificationMethod, params string[] messages);
    }
}