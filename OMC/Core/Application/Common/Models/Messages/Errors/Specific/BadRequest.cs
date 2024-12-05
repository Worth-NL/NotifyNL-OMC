// © 2023, Worth Systems.

using Common.Models.Messages.Base;
using Common.Models.Responses;
using Common.Properties;
using System.Net;

namespace Common.Models.Messages.Errors.Specific
{
    /// <summary>
    /// HTTP request failed.
    /// </summary>
    public abstract record BadRequest
    {
        /// <inheritdoc cref="BadRequest"/>
        /// <seealso cref="BaseSimpleStandardResponseBody"/>
        public sealed record Simplified : BaseSimpleStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Simplified"/> class.
            /// </summary>
            /// <param name="result">The processing result.</param>
            public Simplified(ProcessingResult result)
                : base(HttpStatusCode.BadRequest, AppResources.Operation_ERROR_HttpRequest_Failure, result)
            {
            }
        }

        /// <inheritdoc cref="BadRequest"/>
        /// <seealso cref="BaseEnhancedStandardResponseBody"/>
        public sealed record Detailed : BaseEnhancedStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Detailed"/> class.
            /// </summary>
            /// <param name="result">The processing result.</param>
            public Detailed(ProcessingResult result)
                : base(HttpStatusCode.BadRequest, AppResources.Operation_ERROR_HttpRequest_Failure, result)
            {
            }
        }
    }
}