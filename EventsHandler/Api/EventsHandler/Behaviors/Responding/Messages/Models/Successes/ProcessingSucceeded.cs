// © 2023, Worth Systems.

using EventsHandler.Behaviors.Responding.Messages.Models.Base;
using System.Net;

namespace EventsHandler.Behaviors.Responding.Messages.Models.Successes
{
    /// <summary>
    /// Processing of notification was successful.
    /// </summary>
    /// <seealso cref="BaseEnhancedStandardResponseBody"/>
    internal sealed record ProcessingSucceeded : BaseStandardResponseBody
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingSucceeded"/> class.
        /// </summary>
        /// <param name="processingResult">The description of the notification processing result.</param>
        internal ProcessingSucceeded(string processingResult)
            : base(HttpStatusCode.Accepted, processingResult)
        {
        }
    }
}