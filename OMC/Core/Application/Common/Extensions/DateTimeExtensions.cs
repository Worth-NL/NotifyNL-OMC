// © 2024, Worth Systems.

using System.Globalization;

namespace Common.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="DateTime"/>s.
    /// </summary>
    public static class DateTimeExtensions
    {
        private static readonly TimeZoneInfo s_cetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        private static readonly CultureInfo s_dutchCulture = new("nl-NL");

        private const string DutchDateFormat = "dd-MM-yyyy";

        /// <summary>
        /// Converts the given <see cref="DateTime"/> into a <see langword="string"/> representation of the local (Dutch) date.
        /// </summary>
        /// <param name="dateTime">The source date time.</param>
        /// <returns>
        ///   The <see langword="string"/> representation of a date in the following format:
        ///   <code>15-09-2024</code>
        /// </returns>
        public static string ConvertToDutchDateString(this DateTime dateTime)
        {
            // Convert time zone from UTC to CET (if necessary)
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                dateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, s_cetTimeZone);
            }

            // Formatting the date and time
            return dateTime.ToString(DutchDateFormat, s_dutchCulture);
        }
        
        /// <summary>
        /// Converts the given <see cref="DateOnly"/> into a <see langword="string"/> representation of the local (Dutch) date.
        /// </summary>
        /// <param name="date">The source date.</param>
        /// <returns>
        ///   The <see langword="string"/> representation of a date in the following format:
        ///   <code>15-09-2024</code>
        /// </returns>
        public static string ConvertToDutchDateString(this DateOnly date)
        {
            // Formatting the date and time
            return date.ToString(DutchDateFormat, s_dutchCulture);
        }
    }
}