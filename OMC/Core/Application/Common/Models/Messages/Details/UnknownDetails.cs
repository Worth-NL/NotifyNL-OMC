// © 2023, Worth Systems.

using Common.Constants;
using Common.Models.Messages.Details.Base;

namespace Common.Models.Messages.Details
{
    /// <summary>
    /// Standard format how to display unknown details.
    /// </summary>
    /// <seealso cref="BaseSimpleDetails"/>
    public sealed record UnknownDetails : BaseSimpleDetails
    {
        /// <summary>
        /// Gets the default <see cref="InfoDetails"/>.
        /// </summary>
        public static UnknownDetails Empty { get; } = new()
        {
            Message = CommonValues.Default.Models.DefaultStringValue
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownDetails"/> class.
        /// </summary>
        public UnknownDetails() { }  // NOTE: Used in generic constraints and by object initializer syntax
    }
}