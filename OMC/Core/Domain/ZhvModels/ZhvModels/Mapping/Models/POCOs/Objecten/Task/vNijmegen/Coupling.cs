// © 2024, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Enums.Objecten.vNijmegen;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.Objecten.Task.vNijmegen
{
    /// <summary>
    /// The coupling related to the <see cref="Data"/> retrieved from "Objecten" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version used by Nijmegen.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public struct Coupling : IJsonSerializable
    {
        /// <summary>
        /// The ID of the <see cref="Coupling"/> object (e.g., case).
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("uuid")]
        [JsonPropertyOrder(0)]
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// The type of the <see cref="Coupling"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("registratie")]
        [JsonPropertyOrder(1)]
        public Registrations Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Coupling"/> struct.
        /// </summary>
        public Coupling()
        {
        }
    }
}