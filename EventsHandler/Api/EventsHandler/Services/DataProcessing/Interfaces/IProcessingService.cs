// © 2023, Worth Systems.

using EventsHandler.Mapping.Enums;
using EventsHandler.Mapping.Enums.OpenKlant;
using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json;

namespace EventsHandler.Services.DataProcessing.Interfaces
{
    /// <summary>
    /// The server controlling the workflow how to process received data.
    /// <para>
    ///   This is the heart of the <see cref="EventsHandler"/> business logic.
    /// </para>
    /// </summary>
    public interface IProcessingService<in TData>
        where TData : IJsonSerializable
    {
        /// <summary>
        /// Processes the given input data in a certain way.
        /// </summary>
        /// <param name="data">The data to be processed.</param>
        /// <returns>
        ///   The result of the operation + description of the result.
        /// </returns>
        /// <exception cref="HttpRequestException">
        ///   Something could not be queried from external Web API services.
        /// </exception>
        /// <exception cref="JsonException">The HTTP response wasn't deserialized properly.</exception>
        /// <exception cref="InvalidOperationException">
        ///   Strategy could not be determined or <see cref="DistributionChannels"/> option is invalid.
        /// </exception>
        internal Task<(ProcessingResult, string)> ProcessAsync(TData data);
    }
}