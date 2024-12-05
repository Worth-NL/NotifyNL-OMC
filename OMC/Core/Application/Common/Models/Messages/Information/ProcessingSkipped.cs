// © 2023, Worth Systems.

using Common.Models.Messages.Base;
using Common.Models.Responses;
using System.Net;

namespace Common.Models.Messages.Information
{
    /// <summary>
    /// Processing of notification was skipped (due to some expected reasons).
    /// </summary>
    /// <seealso cref="BaseEnhancedStandardResponseBody"/>
    public sealed record ProcessingSkipped : BaseSimpleStandardResponseBody
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingSkipped"/> class.
        /// </summary>
        /// <param name="result">The processing result.</param>
        public ProcessingSkipped(ProcessingResult result)
            : base(HttpStatusCode.PartialContent, result)
        {
        }
    }
}