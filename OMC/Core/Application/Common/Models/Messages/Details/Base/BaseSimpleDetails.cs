// © 2023, Worth Systems.

using System.Text.Json.Serialization;

namespace Common.Models.Messages.Details.Base
{
    /// <summary>
    /// Standard format how to display details about processed notification.
    /// </summary>
    public abstract record BaseSimpleDetails
    {
        /// <summary>
        /// The message containing a brief summary of the occurred situation.
        /// </summary>
        [JsonPropertyOrder(0)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSimpleDetails"/> class.
        /// </summary>
        protected BaseSimpleDetails() { }

        /// <summary>
        /// <inheritdoc cref="BaseSimpleDetails()"/>
        /// </summary>
        /// <param name="message">The details message.</param>
        protected BaseSimpleDetails(string message)
        {
            this.Message = message;
        }
    }

    /// <summary>
    /// Concrete implementation of <see cref="BaseSimpleDetails"/> allowing to initialize all properties manually.
    /// </summary>
    /// <seealso cref="BaseSimpleDetails"/>
    public sealed record SimpleDetails : BaseSimpleDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDetails"/> class.
        /// </summary>
        /// <param name="message">The details message.</param>
        public SimpleDetails(string message)
            : base(message)
        {
        }
    }
}