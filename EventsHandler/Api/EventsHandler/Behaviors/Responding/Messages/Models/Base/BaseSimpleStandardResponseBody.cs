// © 2023, Worth Systems.

using EventsHandler.Behaviors.Responding.Messages.Models.Details.Base;
using System.Net;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Responding.Messages.Models.Base
{
    /// <summary>
    /// Standard format how to display internal API responses with simplified details.
    /// </summary>
    /// <seealso cref="BaseApiStandardResponseBody"/>
    internal abstract class BaseSimpleStandardResponseBody : BaseApiStandardResponseBody
    {
        [JsonPropertyName("Details")]
        [JsonPropertyOrder(2)]
        public BaseSimpleDetails Details { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSimpleStandardResponseBody"/> class.
        /// </summary>
        protected BaseSimpleStandardResponseBody(HttpStatusCode statusCode, string statusDescription, BaseSimpleDetails details)
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