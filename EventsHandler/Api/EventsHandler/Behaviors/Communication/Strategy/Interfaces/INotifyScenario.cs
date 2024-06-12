// © 2023, Worth Systems.

using EventsHandler.Behaviors.Communication.Strategy.Models.DTOs;
using EventsHandler.Behaviors.Mapping.Enums.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;

namespace EventsHandler.Behaviors.Communication.Strategy.Interfaces
{
    /// <summary>
    /// The "Notify NL" business-cases for notification scenarios.
    /// </summary>
    internal interface INotifyScenario
    {
        /// <summary>
        /// Prepares all data consumed by "Notify NL" API Client.
        /// </summary>
        /// <param name="notification">The notification from "OpenNotificaties" Web API service.</param>
        /// <returns>
        ///   The data required by "Notify NL".
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   The <see cref="DistributionChannels"/> option is invalid.
        /// </exception>
        /// <exception cref="HttpRequestException">
        ///   Something could not be queried from external API Web API services.
        /// </exception>
        internal Task<NotifyData[]> GetAllNotifyDataAsync(NotificationEvent notification);
    }
}