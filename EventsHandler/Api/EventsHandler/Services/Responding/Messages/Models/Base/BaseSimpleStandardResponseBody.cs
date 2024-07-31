// © 2023, Worth Systems.

using EventsHandler.Services.Responding.Messages.Models.Details.Base;
using System.Net;
using System.Text.Json.Serialization;

namespace EventsHandler.Services.Responding.Messages.Models.Base
{
    /// <summary>
    /// Standard format how to display internal API responses with simplified details.
    /// </summary>
    /// <seealso cref="BaseStandardResponseBody"/>
    internal abstract record BaseSimpleStandardResponseBody : BaseStandardResponseBody
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

    /// <summary>
    /// Concrete implementation of <see cref="BaseSimpleStandardResponseBody"/> allowing to initialize all properties manually.
    /// </summary>
    /// <seealso cref="BaseSimpleStandardResponseBody"/>
    internal sealed record SimpleStandardResponseBody : BaseSimpleStandardResponseBody
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleStandardResponseBody"/> class.
        /// </summary>
        internal SimpleStandardResponseBody(HttpStatusCode statusCodes, string statusDescription, BaseSimpleDetails details)
            : base(statusCodes, statusDescription, details)
        {
        }
    }
}