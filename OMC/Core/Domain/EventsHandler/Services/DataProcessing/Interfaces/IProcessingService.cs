// © 2023, Worth Systems.

using Common.Models.Responses;
using System.Text.Json;
using ZhvModels.Mapping.Enums.OpenKlant;

namespace EventsHandler.Services.DataProcessing.Interfaces
{
    /// <summary>
    /// The server controlling the workflow how to process received data.
    /// <para>
    ///   This is the heart of the <see cref="EventsHandler"/> business logic.
    /// </para>
    /// </summary>
    public interface IProcessingService
    {
        /// <summary>
        /// Processes the given input data in a certain way.
        /// </summary>
        /// <param name="json">The data to be processed.</param>
        /// <returns>
        ///   The result of the operation + description of the result + details of processed notification.
        /// </returns>
        /// <exception cref="HttpRequestException">
        ///   Something could not be queried from external Web API services.
        /// </exception>
        /// <exception cref="JsonException">The HTTP response wasn't deserialized properly.</exception>
        /// <exception cref="InvalidOperationException">
        ///   Strategy could not be determined or <see cref="DistributionChannels"/> option is invalid.
        /// </exception>
        internal Task<ProcessingResult> ProcessAsync(object json);
    }
}