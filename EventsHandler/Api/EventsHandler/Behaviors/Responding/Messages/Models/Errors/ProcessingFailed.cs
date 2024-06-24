// © 2023, Worth Systems.

using EventsHandler.Behaviors.Responding.Messages.Models.Base;
using EventsHandler.Behaviors.Responding.Messages.Models.Details.Base;
using System.Net;

namespace EventsHandler.Behaviors.Responding.Messages.Models.Errors
{
    /// <summary>
    /// Processing of notification was unsuccessful (due to some unexpected reasons).
    /// </summary>
    internal static class ProcessingFailed
    {
        /// <inheritdoc cref="ProcessingFailed"/>
        internal sealed record Detailed : BaseEnhancedStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Detailed"/> class.
            /// </summary>
            /// <param name="httpStatusCode">The HTTP Status Code to be used.</param>
            /// <param name="processingResult">The description of the notification processing result.</param>
            /// <param name="details">The details to be included.</param>
            internal Detailed(HttpStatusCode httpStatusCode, string processingResult, BaseEnhancedDetails details)
                : base(httpStatusCode, processingResult, details)
            {
            }
        }

        /// <inheritdoc cref="ProcessingFailed"/>
        /// <seealso cref="BaseStandardResponseBody"/>
        internal sealed record Simplified : BaseStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Simplified"/> class.
            /// </summary>
            /// <param name="httpStatusCode">The HTTP Status Code to be used.</param>
            /// <param name="processingResult">The description of the notification processing result.</param>
            internal Simplified(HttpStatusCode httpStatusCode, string processingResult)
                : base(httpStatusCode, processingResult)
            {
            }
        }
    }
}