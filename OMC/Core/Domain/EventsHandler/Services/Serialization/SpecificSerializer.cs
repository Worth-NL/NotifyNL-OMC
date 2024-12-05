// © 2023, Worth Systems.

using Common.Extensions;
using EventsHandler.Services.Serialization.Converters;
using EventsHandler.Services.Serialization.Interfaces;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;
using ZhvModels.Properties;

namespace EventsHandler.Services.Serialization
{
    /// <inheritdoc cref="ISerializationService"/>
    internal sealed class SpecificSerializer : ISerializationService
    {
        private static readonly ConcurrentDictionary<Type, string> s_cachedRequiredProperties = new();

        #region Custom converters
        private static readonly JsonSerializerOptions s_serializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            
            // They will be applied globally, whenever JSON Serializer Options are used
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
        #endregion

        /// <inheritdoc cref="ISerializationService.Deserialize{TModel}(object)"/>
        TModel ISerializationService.Deserialize<TModel>(object json)
        {
            try
            {
                return JsonSerializer.Deserialize<TModel>($"{json}", s_serializerOptions);
            }
            catch (JsonException exception)
            {
                return GetDeserializationException<TModel>(exception, json);
            }
        }

        /// <inheritdoc cref="ISerializationService.Serialize{TModel}(TModel)"/>
        string ISerializationService.Serialize<TModel>(TModel model)
        {
            return JsonSerializer.Serialize(model, s_serializerOptions);
        }

        #region Helper methods
        private const string EmptyRequired = "_";
        private const string QuotationMark = "'";

        /// <summary>
        /// Gets the human-friendly readable exception with details why the process of deserialization failed.
        /// </summary>
        private static TModel GetDeserializationException<TModel>(JsonException exception, object json)
            where TModel : struct, IJsonSerializable
        {
            string failed;
            string reason;

            if (exception.Path.IsNotNullOrEmpty() &&
                exception.InnerException != null)
            {
                failed = $"{QuotationMark}{exception.Path}{QuotationMark}";
                reason = exception.InnerException.Message;
            }
            else
            {
                failed = $"{QuotationMark}{exception.Message[(exception.Message.IndexOf(':') + 2)..]}{QuotationMark}";
                reason = ZhvResources.Deserialization_ERROR_CannotDeserialize_RequiredProperties;
            }

            throw new JsonException(message:
                string.Format(ZhvResources.Deserialization_ERROR_CannotDeserialize_Message,
                /* {0} - Target */   $"{QuotationMark}{typeof(TModel).Name}.cs{QuotationMark}",
                /* {1} - Failed */   failed,
                /* {2} - Reason */   reason,
                /* {3} - Required */ GetRequiredMembers<TModel>(),
                /* {4} - JSON */     json));
        }

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
                return EmptyRequired;
            }
        }

        private static IEnumerable<string> GetRequiredPropertiesNames(Type type, string parentName = QuotationMark)
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
                    yield return GetRequiredPropertiesNames(requiredProperty.PropertyType, AppendParentName(parentName, requiredProperty)).Join();
                }
                // Case #2: Array
                else if (typeof(IEnumerable).IsAssignableFrom(requiredProperty.PropertyType.BaseType))
                {
                    // Get properties of the array element type
                    yield return GetRequiredPropertiesNames(requiredProperty.PropertyType.GetElementType()!, AppendParentName(parentName, requiredProperty)).Join();
                }
                // Case #3: Generic collection
                else if (requiredProperty.PropertyType.IsGenericType &&  // Prevent exceptions if the simple type is encountered
                         typeof(IEnumerable).IsAssignableFrom(requiredProperty.PropertyType.GetGenericTypeDefinition()))
                {
                    // Get properties of the generic <T> model underlying the generic IEnumerable<T> collection
                    yield return GetRequiredPropertiesNames(requiredProperty.PropertyType.GenericTypeArguments[0], AppendParentName(parentName, requiredProperty)).Join();
                }
                // Case #4: Simple type
                else
                {
                    yield return $"{IncludeParentName(parentName, requiredProperty)}{QuotationMark}";
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