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
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="MijnOverheidResponse"/> indicating the result.</returns>
        Task<MijnOverheidResponse> SendAsync(CloudEvent cloudEvent, CancellationToken cancellationToken = default);
    }
}