// © 2023, Worth Systems.

using EventsHandler.Services.DataProcessing.Strategy.Responses;
using EventsHandler.Services.Responding.Messages.Models.Base;
using System.Net;

namespace EventsHandler.Services.Responding.Messages.Models.Errors
{
    /// <summary>
    /// Processing of notification was unsuccessful (due to some unexpected reasons).
    /// </summary>
    internal static class ProcessingFailed
    {
        /// <inheritdoc cref="ProcessingFailed"/>
        /// <seealso cref="BaseSimpleStandardResponseBody"/>
        internal sealed record Simplified : BaseSimpleStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Simplified"/> class.
            /// </summary>
            /// <param name="statusCode">The HTTP Status Code.</param>
            /// <param name="result">The processing result.</param>
            internal Simplified(HttpStatusCode statusCode, ProcessingResult result)
                : base(statusCode, result)
            {
            }
        }

        /// <inheritdoc cref="ProcessingFailed"/>
        /// <seealso cref="BaseEnhancedStandardResponseBody"/>
        internal sealed record Detailed : BaseEnhancedStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Detailed"/> class.
            /// </summary>
            /// <param name="statusCode">The HTTP Status Code.</param>
            /// <param name="result">The processing result.</param>
            internal Detailed(HttpStatusCode statusCode, ProcessingResult result)
                : base(statusCode, result)
            {
            }
        }
    }
}