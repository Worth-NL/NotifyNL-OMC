﻿// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Properties;
using EventsHandler.Services.Serialization.Interfaces;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

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
                    $"{Resources.Deserialization_ERROR_CannotDeserialize_Value}: {json} | " +
                    $"{Resources.Deserialization_ERROR_CannotDeserialize_Required}: {GetRequiredMembers<TModel>()}");
            }
        }
        
        /// <summary>
        /// Gets text representation of this specific <see cref="IJsonSerializable"/> object.
        /// </summary>
        private static string GetRequiredMembers<TModel>()
            where TModel : struct, IJsonSerializable
        {
            return string.Join(", ", 
                typeof(TModel)
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(property => property.GetCustomAttribute<JsonRequiredAttribute>() != null)
                    .Select(property => property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name)
                    .ToArray());
        }

        /// <inheritdoc cref="ISerializationService.Serialize{TModel}(TModel)"/>
        string ISerializationService.Serialize<TModel>(TModel model)
        {
            return JsonSerializer.Serialize(model);
        }
    }
}