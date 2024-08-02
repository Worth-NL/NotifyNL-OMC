// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json;

namespace EventsHandler.Services.Serialization.Interfaces
{
    /// <summary>
    /// The service to deserialize JSON into &lt;TModel&gt; object and to serialize it back to JSON.
    /// </summary>
    public interface ISerializationService
    {
        /// <summary>
        /// Deserializes the given JSON into a specified object (POCO model).
        /// </summary>
        /// <param name="json">The JSON payload.</param>
        /// <returns>
        ///   Deserialized POCO model.
        /// </returns>
        /// <exception cref="JsonException"/>
        internal TModel Deserialize<TModel>(object json) where TModel : struct, IJsonSerializable;

        /// <summary>
        /// Serializes the given object (POCO model) into a JSON string.
        /// </summary>
        /// <param name="model">The POCO model.</param>
        /// <returns>
        ///   Serialized JSON string.
        /// </returns>
        internal string Serialize<TModel>(TModel model) where TModel : struct, IJsonSerializable;
    }
}