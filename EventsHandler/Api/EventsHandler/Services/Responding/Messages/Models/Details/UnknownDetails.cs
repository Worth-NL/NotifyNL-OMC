// © 2023, Worth Systems.

using EventsHandler.Services.Responding.Messages.Models.Details.Base;

namespace EventsHandler.Services.Responding.Messages.Models.Details
{
    /// <summary>
    /// Standard format how to display unknown details.
    /// </summary>
    /// <seealso cref="BaseSimpleDetails"/>
    internal sealed record UnknownDetails : BaseSimpleDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownDetails"/> class.
        /// </summary>
        public UnknownDetails() { }  // NOTE: Used in generic constraints and by object initializer syntax

        /// <inheritdoc cref="UnknownDetails()"/>
        internal UnknownDetails(string message)
            : base(message)
        {
        }
    }
}