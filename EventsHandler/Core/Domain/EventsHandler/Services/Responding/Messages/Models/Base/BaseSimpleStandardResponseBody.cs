// © 2023, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Services.DataProcessing.Strategy.Responses;
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
        [JsonPropertyOrder(2)]
        public BaseSimpleDetails Details { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSimpleStandardResponseBody"/> class.
        /// </summary>
        /// <param name="statusCode">The HTTP Status Code.</param>
        /// <param name="result">The processing result.</param>
        protected BaseSimpleStandardResponseBody(HttpStatusCode statusCode, ProcessingResult result)
            : base(statusCode, result)
        {
            this.Details = result.Details.Trim();
        }

        /// <summary>
        /// <inheritdoc cref="BaseSimpleStandardResponseBody(HttpStatusCode, ProcessingResult)"/>
        /// </summary>
        /// <param name="statusCode">The HTTP Status Code.</param>
        /// <param name="description">The status description.</param>
        /// <param name="result">The processing result.</param>
        /// <remarks>
        ///   NOTE: <see langword="string"/> "description" is used to replace <see cref="ProcessingResult"/> "result.Description".
        /// </remarks>
        protected BaseSimpleStandardResponseBody(HttpStatusCode statusCode, string description, ProcessingResult result)
            : base(statusCode, description, result)
        {
            this.Details = result.Details.Trim();
        }

        /// <inheritdoc cref="object.ToString()"/>
        public sealed override string ToString()
        {
            return $"{base.ToString()} | {this.Details}";
        }
    }
}