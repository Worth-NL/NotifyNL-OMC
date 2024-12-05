// © 2023, Worth Systems.

using Common.Constants;
using Common.Models.Messages.Details.Base;

namespace Common.Models.Messages.Details
{
    /// <summary>
    /// Standard format how to display information details.
    /// </summary>
    /// <seealso cref="BaseEnhancedDetails"/>
    public sealed record InfoDetails : BaseEnhancedDetails
    {
        /// <summary>
        /// Gets the default <see cref="InfoDetails"/>.
        /// </summary>
        public static InfoDetails Empty { get; } = new
        (
            message: CommonValues.Default.Models.DefaultStringValue,
            cases: CommonValues.Default.Models.DefaultStringValue,
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
        public InfoDetails(string message, string cases, string[] reasons)
            : base(message, cases, reasons)
        {
        }
    }
}