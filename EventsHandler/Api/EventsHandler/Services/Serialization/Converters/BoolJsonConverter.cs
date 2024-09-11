// © 2024, Worth Systems.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventsHandler.Services.Serialization.Converters
{
    /// <summary>
    /// The custom converter specialized in handling <see langword="bool"/> types.
    /// </summary>
    /// <seealso cref="JsonConverter{TValue}" />
    internal sealed class BoolJsonConverter : JsonConverter<bool>
    {
        public override bool HandleNull => true;

        /// <inheritdoc cref="JsonConverter{TValue}.Read(ref Utf8JsonReader, Type, JsonSerializerOptions)"/>
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                    return true;

                case JsonTokenType.False:
                    return false;

                case JsonTokenType.Null:
                    return false;
            }

            return bool.TryParse(reader.GetString(), out bool result) && result;
        }

        /// <inheritdoc cref="JsonConverter{TValue}.Write(Utf8JsonWriter, TValue, JsonSerializerOptions)"/>
        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}