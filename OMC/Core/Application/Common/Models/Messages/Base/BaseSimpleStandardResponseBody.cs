// © 2023, Worth Systems.

using Common.Extensions;
using Common.Models.Messages.Details.Base;
using Common.Models.Responses;
using System.Net;
using System.Text.Json.Serialization;

namespace Common.Models.Messages.Base
{
    /// <summary>
    /// Standard format how to display public API responses with simplified details.
    /// </summary>
    /// <seealso cref="BaseStandardResponseBody"/>
    public abstract record BaseSimpleStandardResponseBody : BaseStandardResponseBody
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
            Details = result.Details.Trim();
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
            Details = result.Details.Trim();
        }

        /// <inheritdoc cref="object.ToString()"/>
        public sealed override string ToString()
        {
            return $"{base.ToString()} | {Details}";
        }
    }
}