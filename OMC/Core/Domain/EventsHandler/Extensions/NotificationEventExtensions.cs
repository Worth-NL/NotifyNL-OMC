// © 2023, Worth Systems.

using EventsHandler.Mapping.Enums.NotificatieApi;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
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
        /// <param name="notification"><inheritdoc cref="NotificationEvent" path="/summary"/></param>
        /// <returns>
        ///   The value of property <see cref="EventAttributes.SourceOrganization"/> ("bronorganisatie").
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   The source organization couldn't be found.
        /// </exception>
        internal static string GetOrganizationId(this NotificationEvent notification)
        {
            string? organizationId = notification.Channel switch
            {
                Channels.Cases     => notification.Attributes.SourceOrganization,
                Channels.Decisions => notification.Attributes.ResponsibleOrganization,
                Channels.Objects   => "missing",  // TODO: Object notification / tasks workflow doesn't have organization id

                _ => null
            };

            // Validation: Wrong type of Channel in notification or the Organization ID is not initialized properly
            return organizationId
                ?? throw new HttpRequestException(ApiResources.HttpRequest_ERROR_NoSourceOrganization + $" [{notification.Channel}]");
        }
    }
}