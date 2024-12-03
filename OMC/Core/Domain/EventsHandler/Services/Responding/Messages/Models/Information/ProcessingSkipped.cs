// © 2023, Worth Systems.

using EventsHandler.Services.DataProcessing.Strategy.Responses;
using EventsHandler.Services.Responding.Messages.Models.Base;
using System.Net;

namespace EventsHandler.Services.Responding.Messages.Models.Information
{
    /// <summary>
    /// Processing of notification was skipped (due to some expected reasons).
    /// </summary>
    /// <seealso cref="BaseEnhancedStandardResponseBody"/>
    internal sealed record ProcessingSkipped : BaseSimpleStandardResponseBody
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingSkipped"/> class.
        /// </summary>
        /// <param name="result">The processing result.</param>
        internal ProcessingSkipped(ProcessingResult result)
            : base(HttpStatusCode.PartialContent, result)
        {
        }
    }
}