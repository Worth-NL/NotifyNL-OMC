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
    /// <seealso cref="BaseSimpleStandardResponseBody"/>
    internal sealed record InternalError : BaseSimpleStandardResponseBody
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InternalError"/> class.
        /// </summary>
        /// <param name="details">The details to be included.</param>
        internal InternalError(BaseSimpleDetails details)
            : base(HttpStatusCode.InternalServerError, Resources.Operation_ERROR_Internal_Unknown, details)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalError"/> class.
        /// </summary>
        /// <param name="details">The details to be included.</param>
        internal InternalError(string details)
            : base(HttpStatusCode.InternalServerError, Resources.Operation_ERROR_Internal_Unknown, new UnknownDetails(details))
        {
        }
    }
}