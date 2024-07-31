// © 2023, Worth Systems.

using EventsHandler.Services.Responding.Messages.Models.Details.Base;

namespace EventsHandler.Services.Responding.Messages.Models.Details
{
    /// <summary>
    /// Standard format how to display error details.
    /// </summary>
    /// <seealso cref="BaseEnhancedDetails"/>
    internal sealed record ErrorDetails : BaseEnhancedDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorDetails"/> class.
        /// </summary>
        public ErrorDetails() { }  // NOTE: Used in generic constraints and by object initializer syntax

        /// <inheritdoc cref="ErrorDetails()"/>
        internal ErrorDetails(string message, string cases, string[] reasons)
            : base(message, cases, reasons)
        {
        }
    }
}