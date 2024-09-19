// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.Objecten.Task;
using EventsHandler.Mapping.Models.POCOs.Objecten.Task.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventsHandler.Services.Serialization.Converters
{
    /// <summary>
    /// The custom converter specialized in handling <see cref="CommonTaskData"/> types.
    /// </summary>
    /// <seealso cref="JsonConverter{TValue}" />
    internal sealed class CommonTaskDataJsonConverter : JsonConverter<CommonTaskData>
    {
        private static readonly object s_padlock = new();
        private static readonly Dictionary<string, object?> s_serializedCommonTaskData = new()
        {
            { nameof(CommonTaskData.CaseUri),        string.Empty },
            { nameof(CommonTaskData.CaseId),         string.Empty },
            { nameof(CommonTaskData.Title),          string.Empty },
            { nameof(CommonTaskData.Status),         string.Empty },
            { nameof(CommonTaskData.ExpirationDate), string.Empty },
            { nameof(CommonTaskData.Identification), string.Empty }
        };

        public override bool HandleNull => true;

        /// <inheritdoc cref="JsonConverter{TValue}.Read(ref Utf8JsonReader, Type, JsonSerializerOptions)"/>
        public override CommonTaskData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                // Case #1: JSON schema used by The Hague
                return JsonSerializer.Deserialize<Mapping.Models.POCOs.Objecten.Task.vHague.TaskObject>(ref reader, options)
                    .ConvertToUnified();
            }
            catch (JsonException)
            {
                // Case #2: JSON schema used by Nijmegen
                return JsonSerializer.Deserialize<Mapping.Models.POCOs.Objecten.Task.vNijmegen.TaskObject>(ref reader, options)
                    .ConvertToUnified();
            }
        }

        /// <inheritdoc cref="JsonConverter{TValue}.Write(Utf8JsonWriter, TValue, JsonSerializerOptions)"/>
        public override void Write(Utf8JsonWriter writer, CommonTaskData value, JsonSerializerOptions options)
        {
            lock (s_padlock)
            {
                s_serializedCommonTaskData[nameof(CommonTaskData.CaseUri)]        = value.CaseUri;
                s_serializedCommonTaskData[nameof(CommonTaskData.CaseId)]         = value.CaseId;
                s_serializedCommonTaskData[nameof(CommonTaskData.Title)]          = value.Title;
                s_serializedCommonTaskData[nameof(CommonTaskData.Status)]         = value.Status;
                s_serializedCommonTaskData[nameof(CommonTaskData.ExpirationDate)] = value.ExpirationDate;
                s_serializedCommonTaskData[nameof(CommonTaskData.Identification)] = value.Identification;

                JsonSerializer.Serialize(writer, s_serializedCommonTaskData, options);
            }
        }
    }
}