// © 2026, Worth Systems.

using WebQueries.DataQuerying.Models.Responses;
using WebQueries.Print.Models;
using ZgwModels.Enums;
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

        /// <summary>
        /// Completes a print job once "Notify NL" reports what happened to the letter: registers the
        /// contactmoment, and removes the triggering object when the letter actually went out.
        /// </summary>
        /// <remarks>
        ///   Deliberately driven by the delivery receipt rather than by the synchronous send, exactly as the
        ///   e-mail and SMS scenarios are. A 201 from "Notify NL" only means the request was accepted; the
        ///   PDF is validated afterwards and can still come back "validation-failed", so a contactmoment
        ///   written at send time could claim a contact that never happened.
        /// </remarks>
        /// <param name="reference">The reference that round-tripped through the delivery receipt.</param>
        /// <param name="notificationMethod">The channel the receipt reports on.</param>
        /// <param name="succeeded">Whether the receipt reports the letter as delivered.</param>
        /// <param name="messages">The contactmoment messages, built by the caller the same way the e-mail path builds them.</param>
        /// <returns>An HTTP response wrapper indicating success or failure.</returns>
        Task<HttpRequestResponse> HandleDeliveryCallbackAsync(
            PrintNotifyReference reference, NotifyMethods notificationMethod, bool succeeded, string[] messages);
    }
}
