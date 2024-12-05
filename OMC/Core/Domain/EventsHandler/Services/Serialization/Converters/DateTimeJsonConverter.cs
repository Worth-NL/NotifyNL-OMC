// © 2024, Worth Systems.

using Common.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventsHandler.Services.Serialization.Converters
{
    /// <summary>
    /// The custom converter specialized in handling <see cref="DateTime"/> types.
    /// </summary>
    /// <seealso cref="JsonConverter{TValue}" />
    internal sealed class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        /// <inheritdoc cref="JsonConverter{TValue}.HandleNull"/>
        public override bool HandleNull => true;

        /// <inheritdoc cref="JsonConverter{TValue}.Read(ref Utf8JsonReader, Type, JsonSerializerOptions)"/>
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType == JsonTokenType.Null 
                ? DateTime.MinValue
                : DateTime.TryParse(reader.GetString(), out DateTime dateTime)
                    ? dateTime
                    : DateTime.MinValue;
        }

        /// <inheritdoc cref="JsonConverter{TValue}.Write(Utf8JsonWriter, TValue, JsonSerializerOptions)"/>
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue($"{value.ConvertToDutchDateString()}");
        }
    }
}