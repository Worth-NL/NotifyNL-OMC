// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak
{
    /// <summary>
    /// The domain object attached to the <see cref="NotificationEvent"/> and used for most of the further references.
    /// <remarks>
    ///   Many workflows might request data from URL stored as "hoofdObject", but the responses also might be different
    ///   depending on the business case scenario. For example, response might be case or decision.
    /// </remarks>
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct MainObject : IJsonSerializable
    {
        /// <summary>
        /// The <see cref="MainObject"/> identifier.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("identificatie")]
        [JsonPropertyOrder(0)]
        public string Id { get; internal set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainObject"/> struct.
        /// </summary>
        public MainObject()
        {
        }
    }
}