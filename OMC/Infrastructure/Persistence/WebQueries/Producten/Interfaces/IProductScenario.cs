// © 2026, Worth Systems.

using WebQueries.DataQuerying.Models.Responses;
using ZgwModels.Mapping.Models.POCOs.NotificatieApi;

namespace WebQueries.Producten.Interfaces
{
    /// <summary>
    /// Handles the "Product created" scenario: fetches the product that "Open Product" reported as created
    /// and notifies each of its owners ("eigenaren") by e-mail.
    /// </summary>
    /// <remarks>
    ///   Kept out of the <c>BaseScenario</c> pipeline on purpose. That pipeline resolves a single party and
    ///   derives the channel from that party's preference in "OpenKlant"; this flow fans out to every
    ///   owner of the product and always uses e-mail.
    /// </remarks>
    public interface IProductScenario
    {
        /// <summary>
        /// Processes a created product: reads it from "Open Product", checks it may be notified about, and
        /// sends one notification per owner, registering a contactmoment on the product either way.
        /// </summary>
        /// <param name="notification">The notification that reported the created product.</param>
        /// <returns>An HTTP response wrapper indicating success or failure.</returns>
        Task<HttpRequestResponse> ProcessProductAsync(NotificationEvent notification);
    }
}
