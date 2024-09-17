// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums.Objecten.vNijmegen;
using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.Objecten.Task.vNijmegen
{
    /// <summary>
    /// The coupling related to the <see cref="TaskObject"/> retrieved from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Coupling : IJsonSerializable
    {
        /// <summary>
        /// The type of the <see cref="Coupling"/>.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("registratie")]
        [JsonPropertyOrder(0)]
        public Registrations Type { get; internal set; }

        /// <summary>
        /// The ID of the <see cref="Coupling"/> object (e.g., case).
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("uuid")]
        [JsonPropertyOrder(1)]
        public Guid Id { get; internal set; } = Guid.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="Coupling"/> struct.
        /// </summary>
        public Coupling()
        {
        }
    }
}