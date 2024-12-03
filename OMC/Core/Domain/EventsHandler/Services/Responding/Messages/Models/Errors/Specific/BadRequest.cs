// © 2023, Worth Systems.

using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Strategy.Responses;
using EventsHandler.Services.Responding.Messages.Models.Base;
using System.Net;

namespace EventsHandler.Services.Responding.Messages.Models.Errors.Specific
{
    /// <summary>
    /// HTTP request failed.
    /// </summary>
    internal abstract record BadRequest
    {
        /// <inheritdoc cref="BadRequest"/>
        /// <seealso cref="BaseSimpleStandardResponseBody"/>
        internal sealed record Simplified : BaseSimpleStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Simplified"/> class.
            /// </summary>
            /// <param name="result">The processing result.</param>
            internal Simplified(ProcessingResult result)
                : base(HttpStatusCode.BadRequest, ApiResources.Operation_ERROR_HttpRequest_Failure, result)
            {
            }
        }
        
        /// <inheritdoc cref="BadRequest"/>
        /// <seealso cref="BaseEnhancedStandardResponseBody"/>
        internal sealed record Detailed : BaseEnhancedStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Detailed"/> class.
            /// </summary>
            /// <param name="result">The processing result.</param>
            internal Detailed(ProcessingResult result)
                : base(HttpStatusCode.BadRequest, ApiResources.Operation_ERROR_HttpRequest_Failure, result)
            {
            }
        }
    }
}