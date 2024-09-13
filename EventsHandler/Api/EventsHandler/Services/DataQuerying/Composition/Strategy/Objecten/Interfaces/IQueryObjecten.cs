﻿// © 2024, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Mapping.Models.POCOs.Objecten.Message;
using EventsHandler.Mapping.Models.POCOs.Objecten.Task;
using EventsHandler.Services.DataQuerying.Composition.Interfaces;
using EventsHandler.Services.DataSending.Clients.Enums;
using EventsHandler.Services.DataSending.Interfaces;
using EventsHandler.Services.DataSending.Responses;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Services.Versioning.Interfaces;
using System.Text.Json;
using Resources = EventsHandler.Properties.Resources;

namespace EventsHandler.Services.DataQuerying.Composition.Strategy.Objecten.Interfaces
{
    /// <summary>
    /// The methods querying specific data from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IVersionDetails"/>
    internal interface IQueryObjecten : IVersionDetails, IDomain
    {
        /// <inheritdoc cref="WebApiConfiguration"/>
        protected internal WebApiConfiguration Configuration { get; set; }

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "Objecten";

        #pragma warning disable CA1822  // These methods can be marked as static but that would be inconsistent for interfaces
        #region Parent (Task)
        /// <summary>
        /// Gets the <see cref="TaskObject"/> from "Objecten" Web API service.
        /// </summary>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException">
        ///   This method might fail when deserializing generic JSON response from Objects endpoint to <see cref="TaskObject"/> model.
        /// </exception>
        internal sealed async Task<TaskObject> GetTaskAsync(IQueryBase queryBase)
        {
            // Request URL
            Uri taskObjectUri = queryBase.Notification.MainObjectUri;

            if (taskObjectUri.IsNotObject())
            {
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotObjectUri);
            }

            return await queryBase.ProcessGetAsync<TaskObject>(
                httpClientType: HttpClientTypes.Objecten,
                uri: taskObjectUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoTask);
        }
        #endregion

        #region Parent (Message)
        /// <summary>
        /// Gets the <see cref="MessageObject"/> from "Objecten" Web API service.
        /// </summary>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="JsonException">
        ///   This method might fail when deserializing generic JSON response from Objects endpoint to <see cref="MessageObject"/> model.
        /// </exception>
        internal sealed async Task<MessageObject> GetMessageAsync(IQueryBase queryBase)
        {
            // Request URL
            Uri taskObjectUri = queryBase.Notification.MainObjectUri;

            if (taskObjectUri.IsNotObject())
            {
                throw new ArgumentException(Resources.Operation_ERROR_Internal_NotObjectUri);
            }

            return await queryBase.ProcessGetAsync<MessageObject>(
                httpClientType: HttpClientTypes.Objecten,
                uri: taskObjectUri,
                fallbackErrorMessage: Resources.HttpRequest_ERROR_NoMessage);
        }
        #endregion
        #pragma warning restore CA1822

        #region Parent (Create message object)        
        /// <summary>
        /// Creates the message object in "Objecten" Web API service.
        /// </summary>
        /// <returns>
        ///   The answer whether the message object was created successfully.
        /// </returns>
        internal sealed async Task<RequestResponse> CreateMessageObjectAsync(IHttpNetworkService networkService, Guid messageObjectTypeGuid, string dataJson)
        {
            // Predefined URL components
            string createObjectEndpoint = $"https://{GetDomain()}/api/v2/objects";

            // Request URL
            Uri createObjectUri = new(createObjectEndpoint);

            // Prepare HTTP Request Body
            string jsonBody = PrepareCreateObjectJson(messageObjectTypeGuid, dataJson);

            return await networkService.PostAsync(
                httpClientType: HttpClientTypes.Objecten,
                uri: createObjectUri,
                jsonBody);
        }

        private string PrepareCreateObjectJson(Guid messageObjectTypeGuid, string dataJson)
        {
            return $"{{" +
                     $"\"type\":\"https://{GetDomain()}/api/v2/objecttypes/{messageObjectTypeGuid}\"," +
                     $"\"record\":{{" +
                       $"\"typeVersion\":\"{this.Configuration.AppSettings.Variables.Objecten.MessageObjectType_Version()}\"," +
                       $"\"data\":{dataJson}," +  // { data } => curly brackets are already included
                       $"\"geometry\":{{" +
                       $"}}," +
                       $"\"startAt\":\"{DateTime.UtcNow}\"," +
                       $"\"correctionFor\":\"string\"" +
                     $"}}" +
                   $"}}";
        }
        #endregion

        #region Polymorphic (Domain)
        /// <inheritdoc cref="IDomain.GetDomain"/>
        string IDomain.GetDomain() => this.Configuration.User.Domain.Objecten();
        #endregion
    }
}