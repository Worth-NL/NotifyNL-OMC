// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Properties;

namespace EventsHandler.Extensions
{
    /// <summary>
    /// Extensions methods for <see cref="NotificationEvent"/>.
    /// </summary>
    internal static class NotificationEventExtensions
    {
        /// <summary>
        /// Gets the organization identifier ("bronorganisatie") from the given <see cref="NotificationEvent"/>.
        /// <para>
        ///   This value is absolutely necessary to continue Web API "OMC" workflow!
        /// </para>
        /// </summary>
        /// <param name="notification">The notification from "OpenNotificaties" Web API service.</param>
        /// <returns>
        ///   The value of property <see cref="EventAttributes.SourceOrganization"/> ("bronorganisatie").
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   The source organization couldn't be found.
        /// </exception>
        internal static string GetOrganizationId(this NotificationEvent notification)
        {
            return notification.Attributes.SourceOrganization
                ?? throw new HttpRequestException(Resources.HttpRequest_ERROR_NoSourceOrganization);
        }
    }
}