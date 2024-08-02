// © 2023, Worth Systems.

using System.Net;
using System.Text.Json.Serialization;

namespace EventsHandler.Services.Responding.Messages.Models.Base
{
    /// <summary>
    /// Standard format how to display internal API responses.
    /// </summary>
    internal abstract record BaseStandardResponseBody
    {
        [JsonPropertyName("Status code")]
        [JsonPropertyOrder(0)]
        public HttpStatusCode StatusCode { get; }

        [JsonPropertyName("Status description")]
        [JsonPropertyOrder(1)]
        public string StatusDescription { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseStandardResponseBody"/> class.
        /// </summary>
        protected BaseStandardResponseBody(HttpStatusCode statusCode, string statusDescription)
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

    /// <summary>
    /// Concrete implementation of <see cref="BaseStandardResponseBody"/> allowing to initialize all properties manually.
    /// </summary>
    /// <seealso cref="BaseStandardResponseBody"/>
    internal sealed record StandardResponseBody : BaseStandardResponseBody
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StandardResponseBody"/> class.
        /// </summary>
        internal StandardResponseBody(HttpStatusCode statusCode, string statusDescription)
            : base(statusCode, statusDescription)
        {
        }
    }
}