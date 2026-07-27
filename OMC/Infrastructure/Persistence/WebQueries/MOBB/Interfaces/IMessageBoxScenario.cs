using System.Text.Json;
using WebQueries.DataQuerying.Models.Responses;

namespace WebQueries.MOBB.Interfaces
{
    /// <summary>
    /// Handles the "Message" scenario: fetches a message from OpenVTB,
    /// validates it, and forwards it to the MOBB / Notify API.
    /// </summary>
    public interface IMessageBoxScenario
    {
        /// <summary>
        /// Processes a CloudEvent by extracting the message UUID from the 'subject' field,
        /// fetching the full message from OpenVTB, validating it, and forwarding it to the MOBB API.
        /// </summary>
        /// <param name="cloudEvent">The full CloudEvent as a JsonElement.</param>
        /// <returns>An HTTP response wrapper indicating success or failure.</returns>
        Task<HttpRequestResponse> ProcessCloudEventAsync(JsonElement cloudEvent);
    }
}
