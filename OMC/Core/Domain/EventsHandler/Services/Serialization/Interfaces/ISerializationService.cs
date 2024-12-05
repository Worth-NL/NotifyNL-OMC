// © 2023, Worth Systems.

using System.Text.Json;
using ZhvModels.Mapping.Models.Interfaces;

namespace EventsHandler.Services.Serialization.Interfaces
{
    /// <summary>
    /// The service to deserialize JSON into &lt;TModel&gt; object and to serialize it back to JSON.
    /// </summary>
    internal interface ISerializationService
    {
        /// <summary>
        /// Deserializes the given JSON into a specified object (POCO model).
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="json">The JSON payload.</param>
        /// <returns>
        ///   Deserialized POCO model.
        /// </returns>
        /// <exception cref="JsonException"/>
        internal TModel Deserialize<TModel>(object json)
            where TModel : struct, IJsonSerializable;

        /// <summary>
        /// Serializes the given object (POCO model) into a JSON string.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="model">The POCO model.</param>
        /// <returns>
        ///   Serialized JSON string.
        /// </returns>
        internal string Serialize<TModel>(TModel model)
            where TModel : struct, IJsonSerializable;
    }
}