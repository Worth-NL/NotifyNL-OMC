// © 2023, Worth Systems.

using Common.Models.Messages.Base;
using Common.Models.Responses;
using System.Net;

namespace Common.Models.Messages.Successes
{
    /// <summary>
    /// Processing of notification was successful.
    /// </summary>
    /// <seealso cref="BaseEnhancedStandardResponseBody"/>
    public sealed record ProcessingSucceeded : BaseSimpleStandardResponseBody
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingSucceeded"/> class.
        /// </summary>
        /// <param name="result">The processing result.</param>
        public ProcessingSucceeded(ProcessingResult result)
            : base(HttpStatusCode.Accepted, result)
        {
        }
    }
}