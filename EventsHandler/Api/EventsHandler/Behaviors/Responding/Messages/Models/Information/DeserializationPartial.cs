// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Behaviors.Responding.Messages.Models.Base;
using EventsHandler.Behaviors.Responding.Messages.Models.Details;
using EventsHandler.Properties;
using System.Net;

namespace EventsHandler.Behaviors.Responding.Messages.Models.Information
{
    /// <summary>
    /// Serialization of <see cref="NotificationEvent"/> was partially successful.
    /// </summary>
    /// <seealso cref="BaseEnhancedStandardResponseBody"/>
    internal sealed class DeserializationPartial : BaseEnhancedStandardResponseBody
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeserializationPartial"/> class.
        /// </summary>
        /// <param name="infoDetails">The details to be included.</param>
        internal DeserializationPartial(InfoDetails infoDetails)
            : base(HttpStatusCode.PartialContent, Resources.Operation_RESULT_Deserialization_Partial, infoDetails)
        {
        }
    }
}