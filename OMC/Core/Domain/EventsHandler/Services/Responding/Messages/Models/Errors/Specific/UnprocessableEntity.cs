// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Properties;
using EventsHandler.Services.DataProcessing.Strategy.Responses;
using EventsHandler.Services.Responding.Messages.Models.Base;
using System.Net;

namespace EventsHandler.Services.Responding.Messages.Models.Errors.Specific
{
    /// <summary>
    /// Serialization of <see cref="NotificationEvent"/> was unsuccessful.
    /// </summary>
    /// <seealso cref="BaseEnhancedStandardResponseBody"/>
    internal abstract record UnprocessableEntity
    {
        /// <inheritdoc cref="UnprocessableEntity"/>
        /// <seealso cref="BaseSimpleStandardResponseBody"/>
        internal sealed record Simplified : BaseSimpleStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Simplified"/> class.
            /// </summary>
            /// <param name="result">The processing result.</param>
            internal Simplified(ProcessingResult result)
                : base(HttpStatusCode.UnprocessableEntity, ApiResources.Operation_ERROR_Deserialization_Failure, result)
            {
            }
        }
        
        /// <inheritdoc cref="UnprocessableEntity"/>
        /// <seealso cref="BaseEnhancedStandardResponseBody"/>
        internal sealed record Detailed : BaseEnhancedStandardResponseBody
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Detailed"/> class.
            /// </summary>
            /// <param name="result">The processing result.</param>
            internal Detailed(ProcessingResult result)
                : base(HttpStatusCode.UnprocessableEntity, ApiResources.Operation_ERROR_Deserialization_Failure, result)
            {
            }
        }
    }
}