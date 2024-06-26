﻿// © 2023, Worth Systems.

using EventsHandler.Properties;
using EventsHandler.Services.Serialization.Interfaces;
using System.Text.Json;

namespace EventsHandler.Services.Serialization
{
    /// <inheritdoc cref="ISerializationService"/>
    internal sealed class SpecificSerializer : ISerializationService
    {
        /// <inheritdoc cref="ISerializationService.Deserialize{TModel}(object)"/>
        TModel ISerializationService.Deserialize<TModel>(object json)
        {
            try
            {
                return JsonSerializer.Deserialize<TModel>($"{json}");
            }
            catch
            {
                throw new JsonException(message:
                    $"{Resources.Deserialization_ERROR_CannotDeserialize_Message} | " +
                    $"{Resources.Deserialization_ERROR_CannotDeserialize_Target}: {typeof(TModel).Name} | " +
                    $"{Resources.Deserialization_ERROR_CannotDeserialize_Value}: {json}");
            }
        }

        /// <inheritdoc cref="ISerializationService.Serialize{TModel}(TModel)"/>
        string ISerializationService.Serialize<TModel>(TModel model)
        {
            return JsonSerializer.Serialize(model);
        }
    }
}