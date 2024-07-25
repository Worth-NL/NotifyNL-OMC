// © 2023, Worth Systems.

using System.Text.Json.Serialization;

namespace EventsHandler.Services.Responding.Messages.Models.Details.Base
{
    /// <summary>
    /// Standard format how to display details about processed notification.
    /// </summary>
    public abstract record BaseSimpleDetails
    {
        /// <summary>
        /// The message containing a brief summary of the occurred situation.
        /// </summary>
        [JsonPropertyName(nameof(Message))]
        [JsonPropertyOrder(0)]
        public string Message { get; internal set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSimpleDetails"/> class.
        /// </summary>
        protected BaseSimpleDetails() { }

        /// <inheritdoc cref="BaseSimpleDetails()"/>
        protected BaseSimpleDetails(string message)
        {
            this.Message = message;
        }
    }

    /// <summary>
    /// Concrete implementation of <see cref="BaseSimpleDetails"/> allowing to initialize all properties manually.
    /// </summary>
    /// <seealso cref="BaseSimpleDetails"/>
    internal sealed record SimpleDetails : BaseSimpleDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDetails"/> class.
        /// </summary>
        internal SimpleDetails(string message)
            : base(message)
        {
        }
    }
}