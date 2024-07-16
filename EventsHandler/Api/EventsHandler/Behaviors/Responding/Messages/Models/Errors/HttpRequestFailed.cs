// © 2023, Worth Systems.

using EventsHandler.Behaviors.Responding.Messages.Models.Base;
using EventsHandler.Behaviors.Responding.Messages.Models.Details.Base;
using EventsHandler.Properties;
using System.Net;

namespace EventsHandler.Behaviors.Responding.Messages.Models.Errors
{
    /// <summary>
    /// A HTTP Request failed.
    /// </summary>
    internal abstract record HttpRequestFailed
    {
        /// <inheritdoc cref="HttpRequestFailed"/>
        /// <seealso cref="BaseEnhancedStandardResponseBody"/>
        internal sealed record Detailed : BaseEnhancedStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Detailed"/> class.
            /// </summary>
            /// <param name="details">The details to be included.</param>
            internal Detailed(BaseEnhancedDetails details)
                : base(HttpStatusCode.BadRequest, Resources.Operation_ERROR_HttpRequest_Failure, details)
            {
            }
        }

        /// <inheritdoc cref="HttpRequestFailed"/>
        /// <seealso cref="BaseSimpleStandardResponseBody"/>
        internal sealed record Simplified : BaseSimpleStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Simplified"/> class.
            /// </summary>
            /// <param name="details">The details to be included.</param>
            internal Simplified(BaseSimpleDetails details)
                : base(HttpStatusCode.BadRequest, Resources.Operation_ERROR_HttpRequest_Failure, details)
            {
            }
        }
    }
}