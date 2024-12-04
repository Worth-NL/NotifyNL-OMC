// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Enums.Objecten;
using ZhvModels.Mapping.Models.Interfaces;
using ZhvModels.Mapping.Models.POCOs.Objecten.Message;
using ZhvModels.Mapping.Models.POCOs.Objecten.Task;

namespace ZhvModels.Mapping.Models.POCOs.Objecten
{
    /// <summary>
    /// The identification related to the different Data (associated with <see cref="CommonTaskData"/> from Task,
    /// <see cref="MessageObject"/>, etc.) retrieved from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Identification : IJsonSerializable
    {
        /// <summary>
        /// The type of the <see cref="Identification"/>.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("type")]
        [JsonPropertyOrder(0)]
        public IdTypes Type { get; public set; }

        /// <summary>
        /// The value of the <see cref="Identification"/>.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("value")]
        [JsonPropertyOrder(1)]
        public string Value { get; public set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="Identification"/> struct.
        /// </summary>
        public Identification()
        {
        }
    }
}