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
    internal abstract record InternalError
    {
        /// <inheritdoc cref="InternalError"/>
        /// <seealso cref="BaseSimpleStandardResponseBody"/>
        internal sealed record Simplified : BaseSimpleStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Simplified"/> class.
            /// </summary>
            /// <param name="result">The processing result.</param>
            internal Simplified(ProcessingResult result)
                : base(HttpStatusCode.InternalServerError, Resources.Operation_ERROR_Internal_Unknown, result)
            {
            }
        }

        /// <inheritdoc cref="InternalError"/>
        /// <seealso cref="BaseEnhancedStandardResponseBody"/>
        internal sealed record Detailed : BaseEnhancedStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Detailed"/> class.
            /// </summary>
            /// <param name="result">The processing result.</param>
            internal Detailed(ProcessingResult result)
                : base(HttpStatusCode.InternalServerError, Resources.Operation_ERROR_Internal_Unknown, result)
            {
            }
        }
    }
}