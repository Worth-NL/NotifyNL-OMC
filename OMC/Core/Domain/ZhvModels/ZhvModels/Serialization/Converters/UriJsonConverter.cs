// © 2024, Worth Systems.

using Common.Constants;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZhvModels.Serialization.Converters
{
    /// <summary>
    /// The custom converter specialized in handling <see cref="Uri"/> types.
    /// </summary>
    /// <seealso cref="JsonConverter{TValue}" />
    internal sealed class UriJsonConverter : JsonConverter<Uri>
    {
        /// <inheritdoc cref="JsonConverter{TValue}.HandleNull"/>
        public override bool HandleNull => true;

        /// <inheritdoc cref="JsonConverter{TValue}.Read(ref Utf8JsonReader, Type, JsonSerializerOptions)"/>
        public override Uri Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Uri.TryCreate(reader.GetString(), UriKind.Absolute, out Uri? uriString)
                ? uriString
                : CommonValues.Default.Models.EmptyUri;
        }

        /// <inheritdoc cref="JsonConverter{TValue}.Write(Utf8JsonWriter, TValue, JsonSerializerOptions)"/>
        public override void Write(Utf8JsonWriter writer, Uri? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.AbsoluteUri ?? CommonValues.Default.Models.EmptyUri.AbsoluteUri);
        }
    }
}