// © 2026, Worth Systems.

namespace ZgwModels.Exceptions
{
    /// <summary>
    /// "OpenKlant" answered the party lookup, and the answer was that no party matches.
    /// </summary>
    /// <remarks>
    ///   Exists to tell a party that is not there apart from a service that could not be reached. Both used
    ///   to surface as a plain <see cref="HttpRequestException"/> without a status code, so a caller had no
    ///   way to decide whether retrying could ever help: for an absent party it cannot, for an outage it must.
    ///   <para>
    ///     Every scenario maps this onto an aborted result (206), so the notification is not redelivered.
    ///     Absent data is fixed in "OpenKlant", not by trying again.
    ///   </para>
    ///   <para>
    ///     Derives from <see cref="HttpRequestException"/> so every existing caller that catches that keeps
    ///     behaving exactly as before. Only callers that want the distinction need to catch this one.
    ///   </para>
    ///   <para>
    ///     The message names the kind of identifier that was searched on, never its value: a BSN is
    ///     personal data, and this message ends up in responses, logs and traces.
    ///   </para>
    /// </remarks>
    public sealed class PartyNotFoundException : HttpRequestException
    {
        /// <summary>
        /// The kind of identifier the lookup was made on, e.g. "bsn", "kvk_nummer" or "uuid". Empty when unknown.
        /// </summary>
        public string IdentifierKind { get; } = string.Empty;

        /// <inheritdoc cref="PartyNotFoundException"/>
        public PartyNotFoundException(string message) : base(message)
        {
        }

        /// <inheritdoc cref="PartyNotFoundException"/>
        public PartyNotFoundException(string message, string identifierKind, Exception? innerException = null)
            : base(message, innerException)
        {
            this.IdentifierKind = identifierKind;
        }

        /// <summary>
        /// No party carries the given "partijIdentificator".
        /// </summary>
        /// <param name="codeSoortObjectId">The kind of identificator searched on, e.g. "bsn" or "kvk_nummer".</param>
        public static PartyNotFoundException ForIdentifier(string codeSoortObjectId)
            => new($"No partij found in OpenKlant for this {Describe(codeSoortObjectId)} " +
                   $"(partijIdentificator \"{codeSoortObjectId}\"). Nobody was notified.",
                codeSoortObjectId);

        /// <summary>
        /// The party a case role points at does not exist.
        /// </summary>
        /// <param name="partyId">The UUID of the party that was referenced. Not personal data, so it is included.</param>
        /// <param name="innerException">The 404 "OpenKlant" answered with.</param>
        public static PartyNotFoundException ForPartyId(Guid partyId, Exception? innerException = null)
            => new($"No partij found in OpenKlant with id {partyId}, although the zaak refers to it. Nobody was notified.",
                "uuid", innerException);

        private static string Describe(string codeSoortObjectId)
            => codeSoortObjectId.ToLowerInvariant() switch
            {
                "bsn" => "BSN",
                "kvk_nummer" => "KVK number",
                "vestigingsnummer" => "vestigingsnummer",
                "rsin" => "RSIN",
                _ => "identifier"
            };
    }
}
