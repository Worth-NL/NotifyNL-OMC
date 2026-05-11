using WebQueries.DataQuerying.Strategies.Interfaces;
using ZhvModels.Mapping.Models.POCOs.Berichten;

namespace WebQueries.DataQuerying.Strategies.Queries.OpenVtb.Interfaces
{
    /// <summary>
    /// The methods for querying specific data from "OpenVTB" Web API service.
    /// </summary>
    public interface IQueryVtb
    {
        /// <summary>
        /// Gets a message by its UUID from the OpenVTB Berichten API.
        /// </summary>
        /// <param name="queryBase">The query base providing the HTTP client context.</param>
        /// <param name="messageUuid">The UUID of the message to retrieve.</param>
        /// <returns>The deserialized message data.</returns>
        Task<MessageData> GetMessageDataAsync(IQueryBase queryBase, Guid messageUuid);
    }
}
