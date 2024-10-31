// © 2023, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Services.Responding.Messages.Models.Details.Base;

namespace EventsHandler.Services.Responding.Messages.Models.Details
{
    /// <summary>
    /// Standard format how to display information details.
    /// </summary>
    /// <seealso cref="BaseEnhancedDetails"/>
    internal sealed record InfoDetails : BaseEnhancedDetails
    {
        /// <summary>
        /// Gets the default <see cref="InfoDetails"/>.
        /// </summary>
        internal static InfoDetails Empty { get; } = new
        (
            message: DefaultValues.Models.DefaultEnumValueName,
            cases:   DefaultValues.Models.DefaultEnumValueName,
            reasons: Array.Empty<string>()
        );

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoDetails"/> class.
        /// </summary>
        public InfoDetails() { }  // NOTE: Used in generic constraints and by object initializer syntax

        /// <inheritdoc cref="InfoDetails()"/>
        internal InfoDetails(string message, string cases, string[] reasons)
            : base(message, cases, reasons)
        {
        }
    }
}