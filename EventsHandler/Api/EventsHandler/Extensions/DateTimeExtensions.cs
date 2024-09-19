// © 2024, Worth Systems.

using System.Globalization;

namespace EventsHandler.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="DateTime"/>s.
    /// </summary>
    internal static class DateTimeExtensions
    {
        private static readonly TimeZoneInfo s_cetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        private static readonly CultureInfo s_dutchCulture = new("nl-NL");

        /// <summary>
        /// Converts the given <see cref="DateTime"/> into a <see langword="string"/> representation of the local (Dutch) date.
        /// </summary>
        /// <param name="dateTime">The source date time.</param>
        /// <returns>
        ///   The <see langword="string"/> representation of a date in the following format:
        ///   <code>2024-09-15</code>
        /// </returns>
        internal static string ConvertToDutchDateString(this DateTime dateTime)
        {
            // Convert time zone from UTC to CET (if necessary)
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                dateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, s_cetTimeZone);
            }

            // Formatting the date and time
            return dateTime.ToString("yyyy-MM-dd", s_dutchCulture);
        }
    }
}