// © 2024, Worth Systems.

using EventsHandler.Behaviors.Communication.Enums;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.Telemetry.Interfaces;

namespace EventsHandler.Services.Telemetry.v2
{
    /// <inheritdoc cref="ITelemetryService"/>
    /// <seealso cref="ITelemetryService"/>
    internal sealed class ContactRegistration : ITelemetryService
    {
        public Task<string> ReportCompletionAsync(NotificationEvent notification, NotifyMethods notificationMethod, string message)
        {
            throw new NotImplementedException();
        }
    }
}