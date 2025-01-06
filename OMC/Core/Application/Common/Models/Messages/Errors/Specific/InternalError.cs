// © 2023, Worth Systems.

using Common.Models.Messages.Base;
using Common.Models.Responses;
using Common.Properties;
using System.Net;

namespace Common.Models.Messages.Errors.Specific
{
    /// <summary>
    /// public server error occurred.
    /// </summary>
    /// <seealso cref="BaseSimpleStandardResponseBody"/>
    public abstract record InternalError
    {
        /// <inheritdoc cref="InternalError"/>
        /// <seealso cref="BaseSimpleStandardResponseBody"/>
        public sealed record Simplified : BaseSimpleStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Simplified"/> class.
            /// </summary>
            /// <param name="result">The processing result.</param>
            public Simplified(ProcessingResult result)
                : base(HttpStatusCode.InternalServerError, CommonResources.Operation_ERROR_Internal, result)
            {
            }
        }

        /// <inheritdoc cref="InternalError"/>
        /// <seealso cref="BaseEnhancedStandardResponseBody"/>
        public sealed record Detailed : BaseEnhancedStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Detailed"/> class.
            /// </summary>
            /// <param name="result">The processing result.</param>
            public Detailed(ProcessingResult result)
                : base(HttpStatusCode.InternalServerError, CommonResources.Operation_ERROR_Internal, result)
            {
            }
        }
    }
}