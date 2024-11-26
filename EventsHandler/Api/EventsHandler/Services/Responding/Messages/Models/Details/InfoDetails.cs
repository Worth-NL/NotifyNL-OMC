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
            message: DefaultValues.Models.DefaultStringValue,
            cases:   DefaultValues.Models.DefaultStringValue,
            reasons: []
        );

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoDetails"/> class.
        /// </summary>
        public InfoDetails() { }  // NOTE: Used in generic constraints and by object initializer syntax

        /// <summary>
        /// <inheritdoc cref="InfoDetails()"/>
        /// </summary>
        /// <param name="message">The details message.</param>
        /// <param name="cases">The cases included in details.</param>
        /// <param name="reasons">The reasons of occurred cases.</param>
        internal InfoDetails(string message, string cases, string[] reasons)
            : base(message, cases, reasons)
        {
        }
    }
}