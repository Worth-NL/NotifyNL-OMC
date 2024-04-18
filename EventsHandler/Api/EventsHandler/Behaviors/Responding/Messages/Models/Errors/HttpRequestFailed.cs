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
    /// <seealso cref="BaseEnhancedStandardResponseBody"/>
    internal sealed class HttpRequestFailed : BaseEnhancedStandardResponseBody
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestFailed"/> class.
        /// </summary>
        /// <param name="details">The details to be included.</param>
        internal HttpRequestFailed(BaseEnhancedDetails details)
            : base(HttpStatusCode.BadRequest, Resources.Operation_RESULT_HttpRequest_Failure, details)
        {
        }
    }
}