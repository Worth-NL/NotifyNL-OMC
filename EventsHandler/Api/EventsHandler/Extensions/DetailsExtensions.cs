// © 2024, Worth Systems.

using EventsHandler.Services.Responding.Messages.Models.Details.Base;

namespace EventsHandler.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="BaseSimpleDetails"/> and <see cref="BaseEnhancedDetails"/>.
    /// </summary>
    internal static class DetailsExtensions
    {
        /// <summary>
        /// Trims the missing details.
        /// </summary>
        /// <param name="details">The enhanced details.</param>
        internal static BaseSimpleDetails Trim(this BaseEnhancedDetails details)
        {
            // Details without Cases and Reasons
            return new SimpleDetails(details.Message);
        }
        
        /// <summary>
        /// Trims the missing details.
        /// </summary>
        /// <param name="details">The enhanced details.</param>
        internal static BaseEnhancedDetails Expand(this BaseSimpleDetails details)
        {
            // Details without Cases and Reasons
            return new EnhancedDetails(details);
        }
    }
}