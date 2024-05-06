// © 2023, Worth Systems.

using EventsHandler.Behaviors.Responding.Messages.Models.Details.Base;
using System.Net;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Responding.Messages.Models.Base
{
    /// <summary>
    /// Standard format how to display internal API responses with elaborative details.
    /// </summary>
    /// <seealso cref="BaseApiStandardResponseBody"/>
    internal abstract class BaseEnhancedStandardResponseBody : BaseApiStandardResponseBody
    {
        [JsonPropertyName("Details")]
        [JsonPropertyOrder(2)]
        public BaseEnhancedDetails Details { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseEnhancedStandardResponseBody"/> class.
        /// </summary>
        protected BaseEnhancedStandardResponseBody(HttpStatusCode statusCode, string statusDescription, BaseEnhancedDetails details)
            : base(statusCode, statusDescription)
        {
            this.Details = details;
        }

        /// <inheritdoc cref="object.ToString()"/>
        public sealed override string ToString()
        {
            return $"Description: {StatusDescription} | Details: {Details.Message}";
        }
    }
}