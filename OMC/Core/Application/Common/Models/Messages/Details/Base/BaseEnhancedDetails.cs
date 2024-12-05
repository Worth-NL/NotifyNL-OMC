// © 2023, Worth Systems.


// © 2023, Worth Systems.

using System.Text.Json.Serialization;

namespace Common.Models.Messages.Details.Base
{
    /// <summary>
    /// Standard format how to display details about processed notification (with additional details).
    /// </summary>
    /// <seealso cref="BaseSimpleDetails"/>
    public abstract record BaseEnhancedDetails : BaseSimpleDetails
    {
        /// <summary>
        /// The comma-separated case(s) that caused the occurred situation.
        /// </summary>
        [JsonPropertyOrder(1)]
        public string Cases { get; set; } = string.Empty;

        /// <summary>
        /// The list of reasons that might be responsible for the occurred situation.
        /// </summary>
        [JsonPropertyOrder(2)]
        public string[] Reasons { get; set; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseEnhancedDetails"/> class.
        /// </summary>
        protected BaseEnhancedDetails() { }

        /// <summary>
        /// <inheritdoc cref="BaseEnhancedDetails()"/>
        /// </summary>
        /// <param name="message">The details message.</param>
        /// <param name="cases">The cases included in details.</param>
        /// <param name="reasons">The reasons of occurred cases.</param>
        protected BaseEnhancedDetails(string message, string cases, string[] reasons)
            : base(message)
        {
            Cases = cases;
            Reasons = reasons;
        }
    }

    /// <summary>
    /// Concrete implementation of <see cref="BaseEnhancedDetails"/> allowing to initialize all properties manually.
    /// </summary>
    /// <seealso cref="BaseEnhancedDetails"/>
    public sealed record EnhancedDetails : BaseEnhancedDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDetails"/> class.
        /// </summary>
        /// <param name="details">The details.</param>
        public EnhancedDetails(BaseSimpleDetails details)
            : base(details.Message, string.Empty, [])
        {
        }
    }
}