﻿// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Versioning.Interfaces;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.ObjectTypen.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "ObjectTypen" Web API service.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    internal interface IQueryObjectTypen : IVersionDetails, IDomain
    {
        /// <inheritdoc cref="WebApiConfiguration"/>
        protected internal WebApiConfiguration Configuration { get; set; }

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "ObjectTypen";

        #region Parent
        /// <summary>
        /// Determines whether the object type is valid.
        /// </summary>
        /// <param name="notification">The initial notification from "OpenNotificaties" Web API service.</param>
        /// <returns>
        ///   <see langword="true"/> if "object type" in the <see cref="NotificationEvent"/> is
        ///   the same as the one defined in the app settings; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="KeyNotFoundException"/>
        internal sealed bool IsValidType(NotificationEvent notification)
        {
            Guid typeGuid = notification.Attributes.ObjectType.GetGuid();

            return typeGuid != default &&
                   typeGuid == this.Configuration.AppSettings.Variables.Objecten.TaskTypeGuid();
        }
        #endregion

        #region Polymorphic (Domain)
        /// <inheritdoc cref="IDomain.GetDomain"/>
        string IDomain.GetDomain() => this.Configuration.User.Domain.ObjectTypen();
        #endregion
    }
}