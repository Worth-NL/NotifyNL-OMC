// © 2023, Worth Systems.

using System.Text.Json.Serialization;

namespace Common.Models.Messages.Details.Base
{
    /// <summary>
    /// Standard format for detailed information about processed notifications.
    /// </summary>
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
        /// Structured field-level validation issues.
        /// </summary>
        [JsonPropertyOrder(3)]
        public Dictionary<string, string>? FieldIssues { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseEnhancedDetails"/> class.
        /// </summary>
        protected BaseEnhancedDetails() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseEnhancedDetails"/> class.
        /// </summary>
        protected BaseEnhancedDetails(string message, string cases, string[] reasons)
            : base(message)
        {
            this.Cases = cases;
            this.Reasons = reasons;
        }
    }

    /// <summary>
    /// Concrete implementation of <see cref="BaseEnhancedDetails"/>.
    /// </summary>
    public sealed record EnhancedDetails : BaseEnhancedDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancedDetails"/> class.
        /// </summary>
        public EnhancedDetails(BaseSimpleDetails details)
            : base(details.Message, string.Empty, [])
        {
        }
    }
}