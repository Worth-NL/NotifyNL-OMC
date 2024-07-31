// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Properties;
using EventsHandler.Services.Responding.Messages.Models.Base;
using EventsHandler.Services.Responding.Messages.Models.Details.Base;
using System.Net;

namespace EventsHandler.Services.Responding.Messages.Models.Errors
{
    /// <summary>
    /// Serialization of <see cref="NotificationEvent"/> was unsuccessful.
    /// </summary>
    /// <seealso cref="BaseEnhancedStandardResponseBody"/>
    internal sealed record DeserializationFailed : BaseEnhancedStandardResponseBody
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeserializationFailed"/> class.
        /// </summary>
        /// <param name="details">The details to be included.</param>
        internal DeserializationFailed(BaseEnhancedDetails details)
            : base(HttpStatusCode.UnprocessableEntity, Resources.Operation_ERROR_Deserialization_Failure, details)
        {
        }
    }
}