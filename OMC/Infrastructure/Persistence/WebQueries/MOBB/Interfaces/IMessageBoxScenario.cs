using System.Text.Json;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.MOBB.Models;
using ZgwModels.Enums;

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

        /// <summary>
        /// Continues the MOBB -> digitale-post (e-mail) -> letter fallback chain from within the
        /// asynchronous Notify NL delivery-receipt callback, for the case where the *initial send call*
        /// was accepted, but the actual delivery later failed (BPMN: the MOBB delivery-outcome gateway
        /// ("(Permanent) Success?") and the email delivery-outcome gateway ("Notificatie gelukt?") - both
        /// evaluate the callback's delivery status, not the synchronous send response already handled by
        /// <see cref="ProcessCloudEventAsync"/>).
        /// </summary>
        /// <param name="reference">The reference that round-tripped through the failed delivery's callback.</param>
        /// <param name="failedChannel">Which channel (as reported by the callback itself) just failed.</param>
        /// <returns>An HTTP response wrapper indicating whether a fallback send was attempted/succeeded.</returns>
        Task<HttpRequestResponse> HandleDeliveryFailureAsync(MessageBoxNotifyReference reference, NotifyMethods failedChannel);
    }
}
