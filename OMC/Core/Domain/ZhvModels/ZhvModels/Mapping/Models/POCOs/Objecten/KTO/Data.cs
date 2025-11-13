// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.Objecten.KTO
{
    /// <summary>
    /// The data related to the <see cref="Message.Record"/> retrieved from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Data : IJsonSerializable
    {
        /// <summary>
        /// The subject of the <see cref="KtoObject"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("ktoobject")]
        [JsonPropertyOrder(0)]
        public string KtoMessage { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="Data"/> struct.
        /// </summary>
        public Data()
        {
        }
    }
}