﻿// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Mapping.Models.POCOs.OpenZaak.Decision;
using System.Text.RegularExpressions;

namespace EventsHandler.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Uri"/>s.
    /// </summary>
    internal static partial class UriExtensions
    {
        #region GUID extraction
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
        #endregion

        #region URI validation
        /// <summary>
        /// Determines whether the given <see cref="Uri"/> doesn't contain <see cref="Case"/> <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The source URI.</param>
        /// <returns>
        ///   <see langword="true"/> if the provided <see cref="Uri"/> IS NOT valid; otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsNotCaseUri(this Uri? uri)
        {
            return uri.DoesNotContain("/zaken/");
        }

        /// <summary>
        /// Determines whether the given <see cref="Uri"/> doesn't contain <see cref="CaseType"/> <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The source URI.</param>
        /// <returns>
        ///   <see langword="true"/> if the provided <see cref="Uri"/> IS NOT valid; otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsNotCaseTypeUri(this Uri? uri)
        {
            return uri.DoesNotContain("/zaaktypen/");
        }

        /// <summary>
        /// Determines whether the given <see cref="Uri"/> doesn't contain <see cref="DecisionResource"/> <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The source URI.</param>
        /// <returns>
        ///   <see langword="true"/> if the provided <see cref="Uri"/> IS NOT valid; otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsNotDecisionResourceUri(this Uri? uri)
        {
            return uri.DoesNotContain("/besluitinformatieobjecten/");
        }

        private static bool DoesNotContain(this Uri? uri, string phrase)
        {
            return !uri?.AbsoluteUri.Contains(phrase) ?? true;
        }
        #endregion
    }
}