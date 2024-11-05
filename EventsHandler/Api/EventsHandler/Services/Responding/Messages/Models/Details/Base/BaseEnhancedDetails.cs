// © 2023, Worth Systems.

using System.Text.Json.Serialization;

namespace EventsHandler.Services.Responding.Messages.Models.Details.Base
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
        [JsonPropertyName(nameof(Cases))]
        [JsonPropertyOrder(1)]
        public string Cases { get; internal set; } = string.Empty;

        /// <summary>
        /// The list of reasons that might be responsible for the occurred situation.
        /// </summary>
        [JsonPropertyName(nameof(Reasons))]
        [JsonPropertyOrder(2)]
        public string[] Reasons { get; internal set; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseEnhancedDetails"/> class.
        /// </summary>
        protected BaseEnhancedDetails() { }

        /// <inheritdoc cref="BaseEnhancedDetails()"/>
        protected BaseEnhancedDetails(string message, string cases, string[] reasons)
            : base(message)
        {
            this.Cases = cases;
            this.Reasons = reasons;
        }
    }
}