// © 2023, Worth Systems.

using EventsHandler.Extensions;
using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Properties;
using EventsHandler.Services.Serialization.Converters;
using EventsHandler.Services.Serialization.Interfaces;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventsHandler.Services.Serialization
{
    /// <inheritdoc cref="ISerializationService"/>
    internal sealed class SpecificSerializer : ISerializationService
    {
        private static readonly ConcurrentDictionary<Type, string> s_cachedRequiredProperties = new();
        private static readonly JsonSerializerOptions s_serializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,

            // Global converters
            Converters =
            {
                new BoolJsonConverter(),
                new CommonTaskDataJsonConverter(),
                new DateOnlyJsonConverter(),
                new DateTimeJsonConverter(),
                new DocumentsJsonConverter(),
                new GuidJsonConverter(),
                new StringJsonConverter(),
                new UriJsonConverter()
            }
        };

        /// <inheritdoc cref="ISerializationService.Deserialize{TModel}(object)"/>
        TModel ISerializationService.Deserialize<TModel>(object json)
        {
            try
            {
                return JsonSerializer.Deserialize<TModel>($"{json}", s_serializerOptions);
            }
            catch (JsonException)
            {
                string requiredProperties = GetRequiredMembers<TModel>();

                throw new JsonException(message:
                    $"{Resources.Deserialization_ERROR_CannotDeserialize_Message} | " +
                    $"{Resources.Deserialization_ERROR_CannotDeserialize_Target}: {typeof(TModel).Name} | " +
                    $"{Resources.Deserialization_ERROR_CannotDeserialize_Value}: {json} | " +
                    $"{Resources.Deserialization_ERROR_CannotDeserialize_Required}: {(requiredProperties.IsNullOrEmpty() ? "_" : requiredProperties)}");
            }
        }

        /// <inheritdoc cref="ISerializationService.Serialize{TModel}(TModel)"/>
        string ISerializationService.Serialize<TModel>(TModel model)
        {
            return JsonSerializer.Serialize(model, s_serializerOptions);
        }

        #region Helper methods
        /// <summary>
        /// Gets text representation of this specific <see cref="IJsonSerializable"/> object.
        /// </summary>
        private static string GetRequiredMembers<TModel>()
            where TModel : struct, IJsonSerializable
        {
            try
            {
                return s_cachedRequiredProperties.GetOrAdd(
                    // Get cached required properties
                    typeof(TModel),
                    // Generate required properties, cache them and return
                    GetRequiredPropertiesNames(typeof(TModel)).Join());
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static IEnumerable<string> GetRequiredPropertiesNames(IReflect type, string parentName = "")
        {
            IEnumerable<PropertyInfo> requiredProperties = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(property => property.GetCustomAttribute<JsonRequiredAttribute>() != null);

            // Return names of properties (Dutch first, C# name as fallback)
            foreach (PropertyInfo requiredProperty in requiredProperties)
            {
                // Case #1: Nested model deriving from IJsonSerializable
                if (typeof(IJsonSerializable).IsAssignableFrom(requiredProperty.PropertyType))
                {
                    // Get properties of the nested model
                    yield return GetRequiredPropertiesNames(requiredProperty.PropertyType, AppendParentName(parentName, requiredProperty))
                                 .Join();
                }
                // Case #2: Collection type
                else if (requiredProperty.PropertyType.IsGenericType &&  // Prevent exceptions if the simple type is encountered
                         typeof(IEnumerable).IsAssignableFrom(requiredProperty.PropertyType.GetGenericTypeDefinition()))
                {
                    // Get properties of the generic <T> model underlying the generic IEnumerable<T> collection
                    yield return GetRequiredPropertiesNames(requiredProperty.PropertyType.GenericTypeArguments[0], AppendParentName(parentName, requiredProperty))
                                 .Join();
                }
                // Case #3: Simple type
                else
                {
                    yield return IncludeParentName(parentName, requiredProperty);
                }
            }
        }

        private static string AppendParentName(string parentName, MemberInfo property)
            => $"{IncludeParentName(parentName, property)}.";

        private static string IncludeParentName(string parentName, MemberInfo property)
            => $"{parentName}{GetPropertyName(property)}";

        private static string GetPropertyName(MemberInfo property)
            => property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
        #endregion
    }
}