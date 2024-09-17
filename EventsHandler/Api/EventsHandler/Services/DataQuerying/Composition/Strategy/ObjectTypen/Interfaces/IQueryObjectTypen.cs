// © 2024, Worth Systems.

using EventsHandler.Extensions;
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

        #region Parent (Create message object)
        /// <summary>
        /// Prepares an object JSON body that can be used by "Objecten" Web API service.
        /// </summary>
        /// <param name="objectTypeId">The object type ID.</param>
        /// <param name="dataJson">The data JSON.</param>
        /// <returns>
        ///   Specific JSON used to define objects from "Objecten" Web API service.
        /// </returns>
        internal string PrepareObjectJsonBody(Guid objectTypeId, string dataJson)
        {
            return $"{{" +
                     $"\"type\":\"https://{GetDomain()}/api/v1/objecttypes/{objectTypeId}\"," +
                     $"\"record\":{{" +
                       $"\"typeVersion\":\"{this.Configuration.AppSettings.Variables.Objecten.MessageObjectType_Version()}\"," +
                       $"\"data\":{{" +
                         $"{dataJson}" +
                       $"}}," +
                       $"\"startAt\":\"{DateTime.UtcNow.ConvertToDutchDateString()}\"" +
                     $"}}" +
                   $"}}";
        }
        #endregion

        #region Polymorphic (Domain)
        /// <inheritdoc cref="IDomain.GetDomain"/>
        string IDomain.GetDomain() => this.Configuration.User.Domain.ObjectTypen();
        #endregion
    }
}