// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums.OpenKlant;

namespace EventsHandler.Behaviors.Communication.Enums
{
    /// <summary>
    /// The notification method used by "Notify NL" API Client to communicate with a citizen.
    /// </summary>
    public enum NotifyMethods
    {
        /// <inheritdoc cref="DistributionChannels.None"/>
        None = DistributionChannels.None,  // 1

        /// <inheritdoc cref="DistributionChannels.Email"/>
        Email = DistributionChannels.Email,  // 2

        /// <inheritdoc cref="DistributionChannels.Sms"/>
        Sms = DistributionChannels.Sms  // 3
    }
}