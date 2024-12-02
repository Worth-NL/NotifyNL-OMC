// © 2023, Worth Systems.

using Common.Constants;
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
        /// Gets the default <see cref="InfoDetails"/>.
        /// </summary>
        internal static ErrorDetails Empty { get; } = new
        (
            message: CommonValues.Default.Models.DefaultStringValue,
            cases:   CommonValues.Default.Models.DefaultStringValue,
            reasons: []
        );

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorDetails"/> class.
        /// </summary>
        public ErrorDetails() { }  // NOTE: Used in generic constraints and by object initializer syntax

        /// <summary>
        /// <inheritdoc cref="ErrorDetails()"/>
        /// </summary>
        /// <param name="message">The details message.</param>
        /// <param name="cases">The cases included in details.</param>
        /// <param name="reasons">The reasons of occurred cases.</param>
        internal ErrorDetails(string message, string cases, string[] reasons)
            : base(message, cases, reasons)
        {
        }
    }
}