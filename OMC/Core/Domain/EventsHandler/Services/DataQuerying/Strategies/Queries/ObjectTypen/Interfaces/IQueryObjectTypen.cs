// © 2024, Worth Systems.

using Common.Extensions;
using Common.Settings.Configuration;
using EventsHandler.Services.DataQuerying.Strategies.Queries;
using EventsHandler.Services.Versioning.Interfaces;

namespace EventsHandler.Services.DataQuerying.Strategies.Queries.ObjectTypen.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "ObjectTypen" Web API service.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    /// <seealso cref="IDomain"/>
    internal interface IQueryObjectTypen : IVersionDetails, IDomain
    {
        /// <inheritdoc cref="WebApiConfiguration"/>
        protected internal WebApiConfiguration Configuration { get; set; }

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "ObjectTypen";

        #region Parent (Create message object)
        /// <summary>
        /// Prepares an object type JSON representation following a schema from "ObjectTypen" Web API service.
        /// </summary>
        /// <param name="dataJson">The data JSON (without outer curly brackets).</param>
        /// <returns>
        ///   The JSON representation of object type.
        /// </returns>
        /// <exception cref="KeyNotFoundException"/>
        internal string PrepareObjectJsonBody(string dataJson)
        {
            return $"{{" +
                     $"\"type\":\"https://{GetDomain()}/objecttypes/{Configuration.ZGW.Variable.ObjectType.MessageObjectType_Uuid()}\"," +
                     $"\"record\":{{" +
                       $"\"typeVersion\":\"{Configuration.ZGW.Variable.ObjectType.MessageObjectType_Version()}\"," +
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
        string IDomain.GetDomain() => Configuration.ZGW.Endpoint.ObjectTypen();
        #endregion
    }
}