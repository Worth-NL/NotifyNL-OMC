using WebQueries.MijnOverheid.Models;

namespace WebQueries.MijnOverheid.Interfaces
{
    /// <summary>
    /// Client for sending CloudEvents to MijnOverheid.
    /// </summary>
    public interface IMijnOverheidClient
    {
        /// <summary>
        /// Sends a CloudEvent to MijnOverheid.
        /// </summary>
        /// <param name="cloudEvent">The event to send.</param>
        /// <returns>True if the request succeeded (2xx status), otherwise false.</returns>
        Task<MijnOverheidResponse> SendAsync(CloudEvent cloudEvent);
    }
}