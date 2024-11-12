// © 2023, Worth Systems.

using EventsHandler.Services.DataProcessing.Strategy.Responses;
using System.Net;
using System.Text.Json.Serialization;

namespace EventsHandler.Services.Responding.Messages.Models.Base
{
    /// <summary>
    /// Standard format how to display internal API responses.
    /// </summary>
    internal abstract record BaseStandardResponseBody
    {
        [JsonPropertyOrder(0)]
        public HttpStatusCode StatusCode { get; }

        [JsonPropertyOrder(1)]
        public string StatusDescription { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseStandardResponseBody"/> class.
        /// </summary>
        /// <param name="statusCode">The HTTP Status Code.</param>
        /// <param name="description">The status description.</param>
        protected BaseStandardResponseBody(HttpStatusCode statusCode, string description)
        {
            this.StatusCode = statusCode;
            this.StatusDescription = description;
        }

        /// <summary>
        /// <inheritdoc cref="BaseStandardResponseBody(HttpStatusCode, string)"/>
        /// </summary>
        /// <param name="statusCode">The HTTP Status Code.</param>
        /// <param name="description">The status description.</param>
        /// <param name="result">The processing result.</param>
        protected BaseStandardResponseBody(HttpStatusCode statusCode, string description, ProcessingResult result)
            : this(statusCode, $"{description} | {result.Description}")
        {
        }

        /// <summary>
        /// <inheritdoc cref="BaseStandardResponseBody(HttpStatusCode, string)"/>
        /// </summary>
        /// <param name="statusCode">The HTTP Status Code.</param>
        /// <param name="result">The processing result.</param>
        protected BaseStandardResponseBody(HttpStatusCode statusCode, ProcessingResult result)
            : this(statusCode, result.Description)
        {
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return $"{(int)this.StatusCode} {this.StatusCode} | {this.StatusDescription}";  // EXAMPLE: "202 Accepted | Operation successful."
        }
    }
}