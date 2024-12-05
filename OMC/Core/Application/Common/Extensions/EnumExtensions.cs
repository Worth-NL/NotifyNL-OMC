// © 2024, Worth Systems.

using Common.Enums.Responses;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Common.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Enum"/>s.
    /// </summary>
    public static class EnumExtensions
    {
        private static readonly ConcurrentDictionary<Enum, string> s_cachedEnumOptionNames = new();

        /// <summary>
        /// Gets the name of the <typeparamref name="TEnum"/> option.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="enum">The enum option.</param>
        /// <returns>
        ///   The custom name of the given <typeparamref name="TEnum"/> option defined in attributes
        ///   or the default enum option name if the expected attributes are not existing.
        /// </returns>
        public static string GetEnumName<TEnum>(this TEnum @enum)
            where TEnum : struct, Enum
        {
            return s_cachedEnumOptionNames.GetOrAdd(
                key: @enum,
                value: ExtractCustomEnumOptionName(@enum));

            static string ExtractCustomEnumOptionName(TEnum @enum)
            {
                string enumOptionName = @enum.ToString();

                string jsonPropertyName =
                    // Case #1: The name from JsonPropertyName attribute can be retrieved
                    typeof(TEnum).GetMember(enumOptionName).FirstOrDefault()?       // The enum option is defined in the given enum
                                 .GetCustomAttribute<JsonPropertyNameAttribute>()?  // The attribute JsonPropertyName is existing
                                 .Name
                    // Case #2: The name from JsonPropertyName attribute cannot be retrieved
                    ?? enumOptionName;

                return !string.IsNullOrWhiteSpace(jsonPropertyName)
                    ? jsonPropertyName
                    // Case #3: The name from JsonPropertyName attribute is empty or contains only white characters
                    : enumOptionName;
            }
        }

        /// <summary>
        /// Converts from <see cref="ProcessingStatus"/> enum to <see cref="LogLevel"/> enum.
        /// </summary>
        /// <param name="processingStatus">The input enum of type A.</param>
        /// <returns>
        ///   The output enum of type B.
        /// </returns>
        public static LogLevel ConvertToLogLevel(this ProcessingStatus processingStatus)
        {
            return processingStatus switch
            {
                ProcessingStatus.Success => LogLevel.Information,

                ProcessingStatus.Skipped or
                ProcessingStatus.Aborted => LogLevel.Warning,

                ProcessingStatus.NotPossible or
                ProcessingStatus.Failure => LogLevel.Error,

                _ => LogLevel.None
            };
        }
    }
}