// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZgwModels.Mapping.Enums.Objecten;
using ZgwModels.Mapping.Models.Interfaces;
using ZgwModels.Mapping.Models.POCOs.Objecten.Message;
using ZgwModels.Mapping.Models.POCOs.Objecten.Task;

namespace ZgwModels.Mapping.Models.POCOs.Objecten
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
        [JsonPropertyName("type")]
        [JsonPropertyOrder(0)]
        public IdTypes Type { get; set; }

        /// <summary>
        /// The value of the <see cref="Identification"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("value")]
        [JsonPropertyOrder(1)]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="Identification"/> struct.
        /// </summary>
        public Identification()
        {
        }
    }
}