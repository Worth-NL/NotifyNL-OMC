// © 2023, Worth Systems.

using Common.Models.Messages.Base;
using Common.Properties;
using System.Net;

namespace Common.Models.Messages.Errors.Specific
{
    /// <summary>
    /// The operation is not implemented.
    /// </summary>
    /// <seealso cref="BaseStandardResponseBody"/>
    public sealed record NotImplemented : BaseStandardResponseBody
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotImplemented"/> class.
        /// </summary>
        public NotImplemented()
            : base(HttpStatusCode.NotImplemented, CommonResources.Operation_ERROR_NotImplemented)
        {
        }
    }
}