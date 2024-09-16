// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Models.POCOs.Objecten.Message;
using EventsHandler.Mapping.Models.POCOs.Objecten.Task;
using EventsHandler.Mapping.Models.POCOs.OpenZaak;
using EventsHandler.Mapping.Models.POCOs.OpenZaak.Decision;
using System.Diagnostics.CodeAnalysis;
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
        /// Determines whether the given <see cref="Uri"/> is <see langword="null"/> or contains <see langword="default"/> value.
        /// </summary>
        /// <param name="uri">The source URI.</param>
        /// <returns>
        ///   <see langword="true"/> if the provided <see cref="Uri"/> is NOT valid; otherwise, <see langword="false"/>.
        /// </returns>
        private static bool IsNullOrDefault([NotNullWhen(false)]this Uri? uri)
        {
            return DefaultValues.Models.EmptyUri.Equals(uri);
        }
        
        /// <summary>
        /// Determines whether the given <see cref="Uri"/> isn't <see langword="null"/> or doesn't contain <see langword="default"/> value.
        /// </summary>
        /// <param name="uri">The source URI.</param>
        /// <returns>
        ///   <see langword="true"/> if the provided <see cref="Uri"/> is valid; otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsNotNullOrDefault([NotNullWhen(true)] this Uri? uri)
        {
            return !uri.IsNullOrDefault();
        }

        /// <summary>
        /// Determines whether the given <see cref="Uri"/> doesn't contain <see cref="Case"/> <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The source URI.</param>
        /// <returns>
        ///   <see langword="true"/> if the provided <see cref="Uri"/> is NOT valid; otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsNotCase(this Uri? uri)
        {
            return uri.DoesNotContain("/zaken/");
        }

        /// <summary>
        /// Determines whether the given <see cref="Uri"/> doesn't contain <see cref="CaseType"/> <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The source URI.</param>
        /// <returns>
        ///   <see langword="true"/> if the provided <see cref="Uri"/> is NOT valid; otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsNotCaseType(this Uri? uri)
        {
            return uri.DoesNotContain("/zaaktypen/");
        }

        /// <summary>
        /// Determines whether the given <see cref="Uri"/> doesn't contain object (e.g., <see cref="CommonData"/> from Task or <see cref="MessageObject"/>) <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The source URI.</param>
        /// <returns>
        ///   <see langword="true"/> if the provided <see cref="Uri"/> is NOT valid; otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsNotObject(this Uri? uri)
        {
            return uri.DoesNotContain("/objects/");
        }

        /// <summary>
        /// Determines whether the given <see cref="Uri"/> doesn't contain <see cref="DecisionResource"/> <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The source URI.</param>
        /// <returns>
        ///   <see langword="true"/> if the provided <see cref="Uri"/> is NOT valid; otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsNotDecisionResource(this Uri? uri)
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