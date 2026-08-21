// © 2026, Worth Systems.

using ZgwModels.Mapping.Enums.Urns;

namespace ZgwModels.Mapping.Models.Urns
{
    /// <summary>
    /// A parsed "betrokkene" URN, identifying who a notification was about.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Two shapes are in circulation, and both are accepted:
    ///     <list type="bullet">
    ///       <item><c>urn:nld:hr:kvk:nummer:12345678</c> - register, then field, then value.</item>
    ///       <item><c>urn:nld:bsn:nummer:123456782</c> - namespace, then field, then value.</item>
    ///     </list>
    ///     Rather than pinning either layout, the namespace is taken from whichever segment names one
    ///     OMC knows, and the value from the final segment - which is where both shapes put it. That
    ///     deliberately does not fit the <c>urn:nld:kvk:{KVK}:{applicatie}</c> form used for CloudEvent
    ///     <em>source</em> URNs, which puts an application name last; those identify a sending system,
    ///     not a betrokkene, and never reach this type.
    ///   </para>
    ///   <para>
    ///     A URN whose namespace is not recognized parses successfully as
    ///     <see cref="UrnNamespaces.Unknown"/> instead of failing, so that callers can report which
    ///     namespace they were handed rather than only that something was wrong.
    ///   </para>
    /// </remarks>
    public readonly struct BetrokkeneUrn
    {
        private const string UrnPrefix = "urn:";

        /// <summary>
        /// The namespace of the identifier this URN carries.
        /// </summary>
        public UrnNamespaces Namespace { get; }

        /// <summary>
        /// The identifier itself - a BSN, a KVK number, and so on.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// The segment that named the <see cref="Namespace"/>, kept verbatim so that an unsupported URN
        /// can be reported with the caller's own wording rather than "unknown".
        /// </summary>
        public string NamespaceSegment { get; }

        private BetrokkeneUrn(UrnNamespaces urnNamespace, string value, string namespaceSegment)
        {
            this.Namespace = urnNamespace;
            this.Value = value;
            this.NamespaceSegment = namespaceSegment;
        }

        /// <summary>
        /// Attempts to parse a "betrokkene" URN.
        /// </summary>
        /// <param name="urn">The raw URN, for example <c>urn:nld:bsn:nummer:123456782</c>.</param>
        /// <param name="result">The parsed URN, when this method returns <see langword="true"/>.</param>
        /// <returns>
        ///   <see langword="true"/> when <paramref name="urn"/> is a syntactically valid URN carrying a
        ///   non-empty value; <see langword="false"/> otherwise. Note that a <see langword="true"/> result
        ///   with <see cref="Namespace"/> set to <see cref="UrnNamespaces.Unknown"/> means the URN was
        ///   well-formed but names a namespace OMC cannot resolve.
        /// </returns>
        public static bool TryParse(string? urn, out BetrokkeneUrn result)
        {
            result = default;

            if (string.IsNullOrWhiteSpace(urn) ||
                !urn.StartsWith(UrnPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string[] segments = urn.Split(':', StringSplitOptions.TrimEntries);

            // "urn", a namespace identifier and a value are the bare minimum for anything usable.
            if (segments.Length < 3)
            {
                return false;
            }

            string value = segments[^1];

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            // Skip segment 0 ("urn") and the trailing value, and look for a namespace OMC knows in what
            // is left - which covers both the "hr:kvk:nummer" and the "bsn:nummer" layouts.
            UrnNamespaces urnNamespace = UrnNamespaces.Unknown;
            string namespaceSegment = segments[1];

            for (int index = 1; index < segments.Length - 1; index++)
            {
                UrnNamespaces recognized = Recognize(segments[index]);

                if (recognized == UrnNamespaces.Unknown)
                {
                    continue;
                }

                urnNamespace = recognized;
                namespaceSegment = segments[index];
                break;
            }

            result = new BetrokkeneUrn(urnNamespace, value, namespaceSegment);

            return true;
        }

        private static UrnNamespaces Recognize(string segment)
        {
            return segment.ToLowerInvariant() switch
            {
                "bsn" => UrnNamespaces.Bsn,
                "kvk" => UrnNamespaces.Kvk,
                _ => UrnNamespaces.Unknown
            };
        }
    }
}
