// © 2023, Worth Systems.

using System.Net;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Responding.Messages.Models.Base
{
    /// <summary>
    /// Standard format how to display internal API responses.
    /// </summary>
    internal abstract class BaseApiStandardResponseBody
    {
        [JsonPropertyName("Status code")]
        [JsonPropertyOrder(0)]
        public HttpStatusCode StatusCode { get; }

        [JsonPropertyName("Status description")]
        [JsonPropertyOrder(1)]
        public string StatusDescription { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseApiStandardResponseBody"/> class.
        /// </summary>
        protected BaseApiStandardResponseBody(HttpStatusCode statusCode, string statusDescription)
        {
            this.StatusCode = statusCode;
            this.StatusDescription = statusDescription;
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return StatusDescription;
        }
    }
}