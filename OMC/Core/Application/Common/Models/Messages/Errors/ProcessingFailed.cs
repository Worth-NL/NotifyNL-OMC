// © 2023, Worth Systems.

using Common.Models.Messages.Base;
using Common.Models.Responses;
using System.Net;

namespace Common.Models.Messages.Errors
{
    /// <summary>
    /// Processing of notification was unsuccessful (due to some unexpected reasons).
    /// </summary>
    public static class ProcessingFailed
    {
        /// <inheritdoc cref="ProcessingFailed"/>
        /// <seealso cref="BaseSimpleStandardResponseBody"/>
        public sealed record Simplified : BaseSimpleStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Simplified"/> class.
            /// </summary>
            /// <param name="statusCode">The HTTP Status Code.</param>
            /// <param name="result">The processing result.</param>
            public Simplified(HttpStatusCode statusCode, ProcessingResult result)
                : base(statusCode, result)
            {
            }
        }

        /// <inheritdoc cref="ProcessingFailed"/>
        /// <seealso cref="BaseEnhancedStandardResponseBody"/>
        public sealed record Detailed : BaseEnhancedStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Detailed"/> class.
            /// </summary>
            /// <param name="statusCode">The HTTP Status Code.</param>
            /// <param name="result">The processing result.</param>
            public Detailed(HttpStatusCode statusCode, ProcessingResult result)
                : base(statusCode, result)
            {
            }
        }
    }
}