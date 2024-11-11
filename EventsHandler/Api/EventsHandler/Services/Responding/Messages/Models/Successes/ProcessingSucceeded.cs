// © 2023, Worth Systems.

using EventsHandler.Services.DataProcessing.Strategy.Responses;
using EventsHandler.Services.Responding.Messages.Models.Base;
using System.Net;

namespace EventsHandler.Services.Responding.Messages.Models.Successes
{
    /// <summary>
    /// Processing of notification was successful.
    /// </summary>
    /// <seealso cref="BaseEnhancedStandardResponseBody"/>
    internal sealed record ProcessingSucceeded : BaseSimpleStandardResponseBody
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingSucceeded"/> class.
        /// </summary>
        /// <param name="result">The processing result.</param>
        internal ProcessingSucceeded(ProcessingResult result)
            : base(HttpStatusCode.Accepted, result)
        {
        }
    }
}