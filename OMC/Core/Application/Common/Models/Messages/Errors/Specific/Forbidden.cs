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
    public abstract record Forbidden
    {
        /// <inheritdoc cref="Forbidden"/>
        /// <seealso cref="BaseSimpleStandardResponseBody"/>
        public sealed record Simplified : BaseSimpleStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Simplified"/> class.
            /// </summary>
            /// <param name="result">The processing result.</param>
            public Simplified(ProcessingResult result)
                : base(HttpStatusCode.Forbidden, AppResources.Operation_ERROR_AccessDenied, result)
            {
            }
        }

        /// <inheritdoc cref="Forbidden"/>
        /// <seealso cref="BaseEnhancedStandardResponseBody"/>
        public sealed record Detailed : BaseEnhancedStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Simplified"/> class.
            /// </summary>
            /// <param name="result">The processing result.</param>
            public Detailed(ProcessingResult result)
                : base(HttpStatusCode.Forbidden, AppResources.Operation_ERROR_AccessDenied, result)
            {
            }
        }
    }
}