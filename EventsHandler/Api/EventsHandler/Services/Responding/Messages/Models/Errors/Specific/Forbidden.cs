// © 2023, Worth Systems.

using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Strategy.Responses;
using EventsHandler.Services.Responding.Messages.Models.Base;
using System.Net;

namespace EventsHandler.Services.Responding.Messages.Models.Errors.Specific
{
    /// <summary>
    /// Internal server error occurred.
    /// </summary>
    /// <seealso cref="BaseSimpleStandardResponseBody"/>
    internal abstract record Forbidden
    {
        /// <inheritdoc cref="Forbidden"/>
        /// <seealso cref="BaseSimpleStandardResponseBody"/>
        internal sealed record Simplified : BaseSimpleStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Simplified"/> class.
            /// </summary>
            /// <param name="result">The processing result.</param>
            internal Simplified(ProcessingResult result)
                : base(HttpStatusCode.Forbidden, Resources.Operation_ERROR_AccessDenied, result)
            {
            }
        }

        /// <inheritdoc cref="Forbidden"/>
        /// <seealso cref="BaseEnhancedStandardResponseBody"/>
        internal sealed record Detailed : BaseEnhancedStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Simplified"/> class.
            /// </summary>
            /// <param name="result">The processing result.</param>
            internal Detailed(ProcessingResult result)
                : base(HttpStatusCode.Forbidden, Resources.Operation_ERROR_AccessDenied, result)
            {
            }
        }
    }
}