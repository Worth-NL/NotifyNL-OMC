// © 2023, Worth Systems.

using EventsHandler.Services.Responding.Messages.Models.Details.Base;
using System.Net;
using System.Text.Json.Serialization;

namespace EventsHandler.Services.Responding.Messages.Models.Base
{
    /// <summary>
    /// Standard format how to display internal API responses with elaborative details.
    /// </summary>
    /// <seealso cref="BaseStandardResponseBody"/>
    internal abstract record BaseEnhancedStandardResponseBody : BaseStandardResponseBody
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
            return $"{StatusDescription} | {Details.Message}";
        }
    }
}