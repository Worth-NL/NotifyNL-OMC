// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums.NotifyNL;

namespace EventsHandler.Behaviors.Communication.Enums.v2
{
    /// <summary>
    /// The statuses mapping "Notify NL" Web API service <see cref="DeliveryStatuses"/> to interpret them trichotomously.
    /// </summary>
    /// <remarks>
    /// NOTE: In "OMC workflow v2" the final communication with the user is based on these shortened statuses.
    /// </remarks>
    public enum NotifyStatuses
    {
        /// <summary>
        /// Default value.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// "Notify NL" Web API service returned one of positive statuses which can be interpreted as success.
        /// </summary>
        Success = 1,

        /// <summary>
        /// "Notify NL" Web API service returned one of ambiguous or internal statuses which should be treated as neutral.
        /// </summary>
        /// <remarks>
        /// NOTE: For logging purposes only!
        /// </remarks>
        Info = 2,

        /// <summary>
        /// "Notify NL" Web API service returned one of negative statuses which can be interpreted as failure.
        /// </summary>
        Failure = 3
    }
}