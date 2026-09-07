// © 2026, Worth Systems.

namespace ZgwModels.Mapping.Enums.Urns
{
    /// <summary>
    /// The identifier namespaces a "betrokkene" URN can carry.
    /// </summary>
    public enum UrnNamespaces
    {
        /// <summary>
        /// The URN did not parse, or names a namespace OMC does not know about.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A BSN (Citizen Service Number), identifying a natural person.
        /// </summary>
        Bsn = 1,

        /// <summary>
        /// A KVK (Chamber of Commerce) number, identifying an organization.
        /// </summary>
        /// <remarks>
        ///   Recognized so that a KVK URN can be rejected with a precise reason rather than a generic
        ///   parse failure. Resolving one to a partij is not supported yet.
        /// </remarks>
        Kvk = 2
    }
}
