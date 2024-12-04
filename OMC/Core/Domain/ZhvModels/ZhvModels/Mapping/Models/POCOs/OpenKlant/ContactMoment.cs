// © 2023, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.OpenKlant
{
    /// <summary>
    /// The registration feedback retrieved from "OpenKlant" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct ContactMoment : IJsonSerializable
    {
        /// <summary>
        /// The reference to the <see cref="ContactMoment"/> in <see cref="Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("url")]
        [JsonPropertyOrder(0)]
        public Uri ReferenceUri { get; public set; } = CommonValues.Default.Models.EmptyUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactMoment"/> struct.
        /// </summary>
        public ContactMoment()
        {
        }
    }
}