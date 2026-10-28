// © 2026, Worth Systems.

namespace WebQueries.Register
{
    /// <summary>
    /// Keeps a klantcontact's "inhoud" (content) within what OpenKlant accepts.
    /// </summary>
    public static class ContactMomentContent
    {
        /// <summary>
        /// The longest "inhoud" OpenKlant accepts.
        /// </summary>
        public const int MaxLength = 1000;

        /// <summary>
        /// Marks content that was cut short.
        /// </summary>
        /// <remarks>
        ///   Three plain ASCII dots, not the ellipsis character (U+2026): that one is 3 bytes in UTF-8 and has
        ///   no Latin-1 equivalent, so downstream systems that count bytes or use an older encoding mangle it.
        /// </remarks>
        public const string Ellipsis = "...";

        /// <summary>
        /// Cuts <paramref name="text"/> to at most <paramref name="maxLength"/> characters, ending in
        /// <see cref="Ellipsis"/> when anything was cut.
        /// </summary>
        public static string Truncate(string text, int maxLength = MaxLength)
        {
            if (text.Length <= maxLength)
            {
                return text;
            }

            if (maxLength <= Ellipsis.Length)
            {
                return Ellipsis[..Math.Max(maxLength, 0)];
            }

            int cut = maxLength - Ellipsis.Length;

            // Never split a surrogate pair: half of one is not a valid character, and serializes as garbage
            if (char.IsHighSurrogate(text[cut - 1]))
            {
                cut--;
            }

            return $"{text[..cut]}{Ellipsis}";
        }
    }
}
