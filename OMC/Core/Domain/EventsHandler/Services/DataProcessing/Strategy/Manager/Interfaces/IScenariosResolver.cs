// © 2024, Worth Systems.

using EventsHandler.Exceptions;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataProcessing.Strategy.Base.Interfaces;
using System.Text.Json;

namespace EventsHandler.Services.DataProcessing.Strategy.Manager.Interfaces
{
    /// <summary>
    /// The strategy manager to control <see cref="INotifyScenario"/>s strategies - determining a specific business workflow.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <typeparam name="TDeterminant">The type of the determinant.</typeparam>
    internal interface IScenariosResolver<TService, in TDeterminant>
        where TService : class
    {
        /// <summary>
        /// Determines which scenario should be used based on the given <typeparamref name="TDeterminant"/>.
        /// </summary>
        /// <param name="notification"><inheritdoc cref="NotificationEvent" path="/summary"/></param>
        /// <returns>
        ///   The resolved <typeparamref name="TService"/> strategy.
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
        /// <exception cref="InvalidOperationException">
        ///   The following service (scenario strategy) could not be resolved from <see cref="IServiceProvider"/>.
        /// </exception>
        /// <exception cref="AbortedNotifyingException">
        ///   The processing and sending of this notification should not be continued.
        /// </exception>
        /// <exception cref="NotImplementedException">
        ///   The processing strategy could not be determined.
        /// </exception>
        internal Task<TService> DetermineScenarioAsync(TDeterminant notification);
    }
}