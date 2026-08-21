// © 2026, Worth Systems.

using WebQueries.DataQuerying.Models.Responses;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;

namespace WebQueries.Print.Interfaces
{
    /// <summary>
    /// Handles the "Print" scenario: fetches a pre-composed PDF referenced by an object in the "Objecten"
    /// Web API service and hands it to "Notify NL" to be printed and posted (the SZW "printstraat" flow).
    /// </summary>
    public interface IPrintScenario
    {
        /// <summary>
        /// Processes a print job: reads the triggering object, fetches the PDF it points at, sends it as a
        /// precompiled letter, registers a contactmoment, and removes the object once it has been printed.
        /// </summary>
        /// <param name="notification">The notification that referenced the print object.</param>
        /// <returns>An HTTP response wrapper indicating success or failure.</returns>
        Task<HttpRequestResponse> ProcessPrintAsync(NotificationEvent notification);
    }
}
