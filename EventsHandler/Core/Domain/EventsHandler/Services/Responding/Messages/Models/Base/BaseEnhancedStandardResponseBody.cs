// © 2023, Worth Systems.

using EventsHandler.Services.DataProcessing.Strategy.Responses;
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
        [JsonPropertyOrder(2)]
        public BaseEnhancedDetails Details { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseEnhancedStandardResponseBody"/> class.
        /// </summary>
        /// <param name="statusCode">The HTTP Status Code.</param>
        /// <param name="result">The processing result.</param>
        protected BaseEnhancedStandardResponseBody(HttpStatusCode statusCode, ProcessingResult result)
            : base(statusCode, result)
        {
            this.Details = result.Details;
        }

        /// <summary>
        /// <inheritdoc cref="BaseEnhancedStandardResponseBody(HttpStatusCode, ProcessingResult)"/>
        /// </summary>
        /// <param name="statusCode">The HTTP Status Code.</param>
        /// <param name="description">The status description.</param>
        /// <param name="result">The processing result.</param>
        /// <remarks>
        ///   NOTE: <see langword="string"/> "description" is used to replace <see cref="ProcessingResult"/> "result.Description".
        /// </remarks>
        protected BaseEnhancedStandardResponseBody(HttpStatusCode statusCode, string description, ProcessingResult result)
            : base(statusCode, description, result)
        {
            this.Details = result.Details;
        }

        /// <inheritdoc cref="object.ToString()"/>
        public sealed override string ToString()
        {
            return $"{base.ToString()} | {this.Details}";
        }
    }
}