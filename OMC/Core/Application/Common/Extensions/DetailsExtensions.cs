// © 2024, Worth Systems.

using Common.Models.Messages.Details.Base;

namespace Common.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="BaseSimpleDetails"/> and <see cref="BaseEnhancedDetails"/>.
    /// </summary>
    public static class DetailsExtensions
    {
        /// <summary>
        /// Trims the missing details.
        /// </summary>
        /// <param name="details">The enhanced details.</param>
        public static BaseSimpleDetails Trim(this BaseEnhancedDetails details)
        {
            // Details without Cases and Reasons
            return new SimpleDetails(details.Message);
        }
        
        /// <summary>
        /// Trims the missing details.
        /// </summary>
        /// <param name="details">The enhanced details.</param>
        public static BaseEnhancedDetails Expand(this BaseSimpleDetails details)
        {
            // Details without Cases and Reasons
            return new EnhancedDetails(details);
        }
    }
}