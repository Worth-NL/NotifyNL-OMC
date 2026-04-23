// © 2023, Worth Systems.

using Common.Models.Messages.Details;
using Common.Models.Messages.Details.Base;
using Common.Models.Responses;
using System.Net;
using System.Text.Json.Serialization;

namespace Common.Models.Messages.Base
{
    /// <summary>
    /// Standard format how to display OMC Web API responses with elaborative details.
    /// </summary>
    /// <seealso cref="BaseStandardResponseBody"/>
    public abstract record BaseEnhancedStandardResponseBody : BaseStandardResponseBody
    {
        /// <summary>
        /// Gets response details (enhanced).
        /// </summary>
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
        /// Initializes a new instance of the <see cref="BaseEnhancedStandardResponseBody"/> class.
        /// </summary>
        /// <param name="statusCode">The HTTP Status Code.</param>
        /// <param name="description">The status description.</param>
        /// <param name="result">The processing result.</param>
        /// <remarks>
        /// NOTE: <see langword="string"/> "description" replaces <see cref="ProcessingResult.Description"/>.
        /// </remarks>
        protected BaseEnhancedStandardResponseBody(HttpStatusCode statusCode, string description, ProcessingResult result)
            : base(statusCode, description, result)
        {
            this.Details = result.Details;
        }

        /// <inheritdoc cref="object.ToString()"/>
        public sealed override string ToString()
        {
            string detailsText = this.Details?.ToString() ?? "No details provided";

            return $"{base.ToString()} | {detailsText}";
        }
    }
}