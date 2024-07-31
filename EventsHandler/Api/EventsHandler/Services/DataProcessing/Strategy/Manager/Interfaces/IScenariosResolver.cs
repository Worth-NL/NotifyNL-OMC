// © 2024, Worth Systems.

using EventsHandler.Exceptions;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataProcessing.Strategy.Interfaces;
using System.Text.Json;

namespace EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces
{
    /// <summary>
    /// The strategy manager to control <see cref="INotifyScenario"/>s strategies - determining a specific business workflow.
    /// </summary>
    internal interface IScenariosResolver
    {
        /// <summary>
        /// Determines which workflow scenario should be used based on the delivered <see cref="NotificationEvent"/>.
        /// </summary>
        /// <param name="notification">The notification from "OpenNotificaties" Web API service.</param>
        /// <returns>
        ///   The appropriate <see cref="INotifyScenario"/> strategy.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        ///   The key used to retrieve a specific "environment variable" value was invalid.
        /// </exception>
        /// <exception cref="HttpRequestException">
        ///   There was an error while attempting to fetch some resources using HTTP Request.
        /// </exception>
        /// <exception cref="JsonException">
        ///   The JSON payload could not be deserialized into specified POCO model.
        /// </exception>
        /// <exception cref="AbortedNotifyingException">
        ///   The processing and sending of this notification should not be continued.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   The following service (scenario strategy) could not be resolved from <see cref="IServiceProvider"/>.
        /// </exception>
        /// <exception cref="NotImplementedException">
        ///   The processing strategy could not be determined.
        /// </exception>
        internal Task<INotifyScenario> DetermineScenarioAsync(NotificationEvent notification);
    }
}
