// © 2023, Worth Systems.

using EventsHandler.Behaviors.Responding.Messages.Models.Base;
using EventsHandler.Properties;
using System.Net;

namespace EventsHandler.Behaviors.Responding.Messages.Models.Errors
{
    /// <summary>
    /// The operation is not implemented.
    /// </summary>
    /// <seealso cref="BaseStandardResponseBody"/>
    internal sealed record NotImplemented : BaseStandardResponseBody
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotImplemented"/> class.
        /// </summary>
        internal NotImplemented()
            : base(HttpStatusCode.NotImplemented, Resources.Operation_RESULT_NotImplemented)
        {
        }
    }
}