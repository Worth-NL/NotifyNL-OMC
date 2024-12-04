// © 2024, Worth Systems.

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Decision;

namespace EventsHandler.Services.Serialization.Converters
{
    /// <summary>
    /// The custom converter specialized in handling <see cref="ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision.Documents"/> types.
    /// </summary>
    /// <seealso cref="JsonConverter{TValue}" />
    internal sealed class DocumentsJsonConverter : JsonConverter<Documents>
    {
        private static readonly object s_padlock = new();
        private static readonly Dictionary<string, List<Document>> s_serializedDocuments = [];
        private static string? s_resultsJsonPropertyName;

        public override bool HandleNull => true;

        /// <inheritdoc cref="JsonConverter{TValue}.Read(ref Utf8JsonReader, Type, JsonSerializerOptions)"/>
        public override Documents Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new Documents
            {
                Results = JsonSerializer.Deserialize<List<Document>>(ref reader, options) ?? []
            };
        }

        /// <inheritdoc cref="JsonConverter{TValue}.Write(Utf8JsonWriter, TValue, JsonSerializerOptions)"/>
        public override void Write(Utf8JsonWriter writer, Documents value, JsonSerializerOptions options)
        {
            lock (s_padlock)
            {
                // Determine JSON property name
                s_resultsJsonPropertyName ??= typeof(Documents)
                    .GetProperty(nameof(Documents.Results), BindingFlags.Instance | BindingFlags.Public)!
                    .GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? nameof(Documents.Results);

                // Prepare JSON key-value pair
                s_serializedDocuments[s_resultsJsonPropertyName] = value.Results;

                // Serializing List<Document> models to later serialize them into Documents model
                JsonSerializer.Serialize(writer, s_serializedDocuments, options);
            }
        }
    }
}