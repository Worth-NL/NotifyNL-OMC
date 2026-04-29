using WebQueries.MijnOverheid.Models;

namespace WebQueries.MijnOverheid.Interfaces
{
    /// <summary>
    /// Forwards CloudEvents to MijnOverheid after applying business rules.
    /// </summary>
    public interface IMijnOverheidForwarder
    {
        /// <summary>
        /// Processes an incoming CloudEvent and sends it to MijnOverheid if conditions are met.
        /// </summary>
        /// <param name="cloudEvent">The CloudEvent received from ZGW.</param>
        /// <returns>
        /// The response from MijnOverheid if a send was attempted; otherwise null if the event was skipped.
        /// </returns>
        Task<MijnOverheidResponse?> ForwardIfNeededAsync(CloudEvent cloudEvent);
    }
}