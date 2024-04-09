// © 2023, Worth Systems.

using EventsHandler.Behaviors.Responding.Messages.Models.Base;
using EventsHandler.Behaviors.Responding.Messages.Models.Details;
using EventsHandler.Behaviors.Responding.Messages.Models.Details.Base;
using EventsHandler.Properties;
using System.Net;

namespace EventsHandler.Behaviors.Responding.Messages.Models.Errors
{
    /// <summary>
    /// An internal server error occurred.
    /// </summary>
    /// <seealso cref="BaseApiStandardResponseBody"/>
    internal sealed class InternalError : BaseSimpleStandardResponseBody
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InternalError"/> class.
        /// </summary>
        /// <param name="details">The details to be included.</param>
        internal InternalError(BaseSimpleDetails details)
            : base(HttpStatusCode.InternalServerError, Resources.Operation_RESULT_Internal, details)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="InternalError"/> class.
        /// </summary>
        /// <param name="details">The details to be included.</param>
        internal InternalError(string details)
            : base(HttpStatusCode.InternalServerError, Resources.Operation_RESULT_Internal, new UnknownDetails(details))
        {
        }
    }
}