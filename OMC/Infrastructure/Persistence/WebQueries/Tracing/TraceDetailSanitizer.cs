// © 2026, Worth Systems.

using System.Text.RegularExpressions;

namespace WebQueries.Tracing
{
    /// <summary>
    /// Scrubs a trace event's detail before it leaves the process.
    /// </summary>
    /// <remarks>
    ///   The live trace stream (<c>/status/trace/stream</c>) is served without authentication, so
    ///   anything in a detail is effectively public. Most details are fixed text, but many call sites
    ///   pass an exception message on failure, and a failed register call's message carries the full
    ///   request URI and the response body (see <c>QueryBase.GetMessage</c>) — for a party search in
    ///   OpenKlant that URI holds the BSN or KVK number being searched for. Scrubbing here, in the one
    ///   place every detail passes through, covers every scenario and every future call site, instead
    ///   of relying on each of them to remember.
    /// </remarks>
    internal static partial class TraceDetailSanitizer
    {
        /// <summary>
        /// The separator <c>QueryBase.GetMessage</c> puts between its readable message and the
        /// request URI and response body that follow it.
        /// </summary>
        private const string QueryBaseUriMarker = " | URI: ";

        private const int MaxLength = 300;
        private const string Masked = "[…]";

        /// <summary>
        /// Returns the detail without anything that could identify a person: the request URI and
        /// response body of a failed register call, URL query strings, and long digit runs (a BSN is 9
        /// digits, a KVK number 8, a vestigingsnummer 12). UUIDs are left alone. The result is capped
        /// in length, since some details carry input from outside (e.g. a CloudEvent type).
        /// </summary>
        public static string? Sanitize(string? detail)
        {
            if (string.IsNullOrEmpty(detail))
            {
                return detail;
            }

            string result = detail;

            int uriMarker = result.IndexOf(QueryBaseUriMarker, StringComparison.Ordinal);
            if (uriMarker >= 0)
            {
                result = result[..uriMarker];
            }

            result = QueryString().Replace(result, "$1");
            result = LongDigitRun().Replace(result, Masked);

            return result.Length > MaxLength
                ? $"{result[..MaxLength]}…"
                : result;
        }

        /// <summary>An http(s) URL followed by a query string or fragment; group 1 is the URL without it.</summary>
        [GeneratedRegex(@"(https?://[^\s?#]+)[?#]\S*", RegexOptions.IgnoreCase)]
        private static partial Regex QueryString();

        /// <summary>
        /// Eight or more digits not part of a larger word or of a hyphenated token — so a BSN, KVK or
        /// vestigingsnummer is caught, but an all-digit segment of a UUID is not.
        /// </summary>
        [GeneratedRegex(@"(?<![\w-])\d{8,}(?![\w-])")]
        private static partial Regex LongDigitRun();
    }
}
