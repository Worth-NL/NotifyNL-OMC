// © 2024, Worth Systems.

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Common.Constants;

namespace ZhvModels.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Uri"/>s.
    /// </summary>
    public static partial class UriExtensions
    {
        #region GUID extraction
        [GeneratedRegex("\\w{8}\\-\\w{4}\\-\\w{4}\\-\\w{4}\\-\\w{12}", RegexOptions.Compiled | RegexOptions.RightToLeft)]
        private static partial Regex GuidRegexPattern();

        /// <summary>
        /// Extracts <see cref="Guid"/> (UUID) from the given <see cref="Uri.AbsoluteUri"/>.
        /// </summary>
        /// <param name="uri">The source URI.</param>
        public static Guid GetGuid(this Uri? uri)
        {
            if (uri == null ||
                uri == CommonValues.Default.Models.EmptyUri)
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
        public static bool IsNullOrDefault([NotNullWhen(false)] this Uri? uri)
        {
            return CommonValues.Default.Models.EmptyUri.Equals(uri);
        }
        
        /// <summary>
        /// Determines whether the given <see cref="Uri"/> isn't <see langword="null"/> or doesn't contain <see langword="default"/> value.
        /// </summary>
        /// <param name="uri">The source URI.</param>
        /// <returns>
        ///   <see langword="true"/> if the provided <see cref="Uri"/> is valid; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsNotNullOrDefault([NotNullWhen(true)] this Uri? uri)
        {
            return !uri.IsNullOrDefault();
        }

        /// <summary>
        /// Determines whether the given <see cref="Uri"/> doesn't contain <see cref="ZhvModels.Mapping.Models.POCOs.OpenZaak.Case"/> <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The source URI.</param>
        /// <returns>
        ///   <see langword="true"/> if the provided <see cref="Uri"/> is NOT valid; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsNotCase(this Uri? uri)
        {
            return uri.DoesNotContain("/zaken");
        }

        /// <summary>
        /// Determines whether the given <see cref="Uri"/> doesn't contain <see cref="ZhvModels.Mapping.Models.POCOs.OpenZaak.CaseType"/> <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The source URI.</param>
        /// <returns>
        ///   <see langword="true"/> if the provided <see cref="Uri"/> is NOT valid; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsNotCaseType(this Uri? uri)
        {
            return uri.DoesNotContain("/zaaktypen");
        }

        /// <summary>
        /// Determines whether the given <see cref="Uri"/> doesn't contain party result <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The source URI.</param>
        /// <returns>
        ///   <see langword="true"/> if the provided <see cref="Uri"/> is NOT valid; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsNotParty(this Uri? uri)
        {
            return uri.DoesNotContain("/partijen");
        }

        /// <summary>
        /// Determines whether the given <see cref="Uri"/> doesn't contain object (e.g., <see cref="ZhvModels.Mapping.Models.POCOs.Objecten.Task.CommonTaskData"/> from Task or <see cref="ZhvModels.Mapping.Models.POCOs.Objecten.Message.MessageObject"/>) <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The source URI.</param>
        /// <returns>
        ///   <see langword="true"/> if the provided <see cref="Uri"/> is NOT valid; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsNotObject(this Uri? uri)
        {
            return uri.DoesNotContain("/objects");
        }

        /// <summary>
        /// Determines whether the given <see cref="Uri"/> doesn't contain <see cref="ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.DecisionResource"/> <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The source URI.</param>
        /// <returns>
        ///   <see langword="true"/> if the provided <see cref="Uri"/> is NOT valid; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsNotDecisionResource(this Uri? uri)
        {
            return uri.DoesNotContain("/besluitinformatieobjecten");
        }

        private static bool DoesNotContain(this Uri? uri, string phrase)
        {
            return !uri?.AbsoluteUri.Contains(phrase) ?? true;
        }
        #endregion
    }
}