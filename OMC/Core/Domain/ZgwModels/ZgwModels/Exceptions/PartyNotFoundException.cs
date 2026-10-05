// © 2026, Worth Systems.

namespace ZgwModels.Exceptions
{
    /// <summary>
    /// "OpenKlant" answered the party search, and the answer was that no party matches.
    /// </summary>
    /// <remarks>
    ///   Exists to tell a party that is not there apart from a service that could not be reached. Both used
    ///   to surface as a plain <see cref="HttpRequestException"/> without a status code, so a caller had no
    ///   way to decide whether retrying could ever help: for an absent party it cannot, for an outage it must.
    ///   <para>
    ///     Derives from <see cref="HttpRequestException"/> so every existing caller that catches that keeps
    ///     behaving exactly as before. Only callers that want the distinction need to catch this one.
    ///   </para>
    /// </remarks>
    public sealed class PartyNotFoundException : HttpRequestException
    {
        /// <inheritdoc cref="PartyNotFoundException"/>
        public PartyNotFoundException(string message) : base(message)
        {
        }
    }
}
