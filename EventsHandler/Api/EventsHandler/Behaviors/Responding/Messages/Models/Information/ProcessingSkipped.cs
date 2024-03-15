// © 2023, Worth Systems.

using EventsHandler.Behaviors.Responding.Messages.Models.Base;
using EventsHandler.Behaviors.Responding.Messages.Models.Details.Base;
using System.Net;

namespace EventsHandler.Behaviors.Responding.Messages.Models.Information
{
    /// <summary>
    /// Processing of notification was skipped (due to some expected reasons).
    /// </summary>
    /// <seealso cref="BaseEnhancedStandardResponseBody"/>
    internal sealed class ProcessingSkipped : BaseEnhancedStandardResponseBody
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingSkipped"/> class.
        /// </summary>
        /// <param name="processingResult">The description of the notification processing result.</param>
        /// <param name="details">The details to be included.</param>
        internal ProcessingSkipped(string processingResult, BaseEnhancedDetails details)
            : base(HttpStatusCode.PartialContent, processingResult, details)
        {
        }
    }
}