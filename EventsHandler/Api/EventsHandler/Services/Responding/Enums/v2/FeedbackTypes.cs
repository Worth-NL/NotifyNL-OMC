// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums.NotifyNL;

namespace EventsHandler.Services.Responding.Enums.v2
{
    /// <summary>
    /// The types of feedback mapping "Notify NL" Web API service <see cref="DeliveryStatuses"/>.
    /// </summary>
    /// <remarks>
    /// NOTE: In "OMC workflow v2" the final communication with the user is based on this value.
    /// </remarks>
    public enum FeedbackTypes
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
        /// "Notify NL" Web API service returned one of ambiguous or internal statuses which should be treated as info.
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