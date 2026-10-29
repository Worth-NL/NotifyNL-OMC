// © 2026, Worth Systems.

using System.Text.RegularExpressions;

namespace WebQueries.Logging
{
    /// <summary>
    /// Masks the numbers that identify a person in text that leaves the process, such as a Sentry message.
    /// </summary>
    /// <remarks>
    ///   A register call's URI and response can carry a BSN, a KVK number or a vestigingsnummer, e.g. an OpenKlant
    ///   party search has it in its query string. Everything else is left as it is, so an error stays readable.
    /// </remarks>
    public static partial class PersonalDataScrubber
    {
        /// <summary>
        /// What a masked number is replaced with.
        /// </summary>
        public const string Masked = "[…]";

        /// <summary>
        /// Returns the text with every run of 8 or more digits masked (a BSN is 9 digits, a KVK number 8, a
        /// vestigingsnummer 12). Digits that are part of a larger word or a hyphenated token, such as a UUID, are
        /// left alone.
        /// </summary>
        public static string? Scrub(string? text)
            => string.IsNullOrEmpty(text) ? text : LongDigitRun().Replace(text, Masked);

        /// <summary>
        /// Eight or more digits not part of a larger word or of a hyphenated token - so a BSN, KVK or
        /// vestigingsnummer is caught, but an all-digit segment of a UUID is not.
        /// </summary>
        [GeneratedRegex(@"(?<![\w-])\d{8,}(?![\w-])")]
        private static partial Regex LongDigitRun();
    }
}
