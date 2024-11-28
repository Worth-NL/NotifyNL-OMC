// © 2023, Worth Systems.

using EventsHandler.Properties;
using EventsHandler.Services.Responding.Messages.Models.Base;
using System.Net;

namespace EventsHandler.Services.Responding.Messages.Models.Errors.Specific
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
            : base(HttpStatusCode.NotImplemented, ApiResources.Operation_ERROR_NotImplemented)
        {
        }
    }
}