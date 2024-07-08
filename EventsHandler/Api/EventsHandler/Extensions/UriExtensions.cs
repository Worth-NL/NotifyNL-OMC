// © 2024, Worth Systems.

using EventsHandler.Constants;
using System.Text.RegularExpressions;

namespace EventsHandler.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Uri"/>s.
    /// </summary>
    internal static partial class UriExtensions
    {
        [GeneratedRegex("\\w{8}\\-\\w{4}\\-\\w{4}\\-\\w{4}\\-\\w{12}", RegexOptions.Compiled | RegexOptions.RightToLeft)]
        private static partial Regex GuidRegexPattern();

        /// <summary>
        /// Extracts <see cref="Guid"/> (UUID) from the given <see cref="Uri.AbsoluteUri"/>.
        /// </summary>
        /// <param name="uri">The source URI.</param>
        internal static Guid GetGuid(this Uri? uri)
        {
            if (uri == null ||
                uri == DefaultValues.Models.EmptyUri)
            {
                return Guid.Empty;
            }

            Match guidMatch = GuidRegexPattern().Match(uri.AbsoluteUri);

            return guidMatch.Success
                ? new Guid(guidMatch.Value)
                : Guid.Empty;
        }
    }
}