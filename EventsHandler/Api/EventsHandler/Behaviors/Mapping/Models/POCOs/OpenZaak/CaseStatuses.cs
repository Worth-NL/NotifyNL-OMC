// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;
using EventsHandler.Properties;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenZaak
{
    /// <summary>
    /// The statuses of the <see cref="Case"/> retrieved from "OpenZaak" Web service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseStatuses : IJsonSerializable
    {
        /// <summary>
        /// The number of received results.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("count")]
        [JsonPropertyOrder(0)]
        public int Count { get; internal set; }

        /// <inheritdoc cref="CaseStatus"/>
        [JsonInclude]
        [JsonPropertyName("results")]
        [JsonPropertyOrder(1)]
        public List<CaseStatus> Results { get; internal set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseStatuses"/> struct.
        /// </summary>
        public CaseStatuses()
        {
        }

        /// <summary>
        /// Determines whether the <see cref="Case"/> has only 1
        /// <see cref="CaseStatus"/> => it wasn't updated yet.
        /// </summary>
        internal readonly bool WereNeverUpdated() => this.Count == 1;

        /// <summary>
        /// Gets the newest <see cref="CaseStatus"/> from multiple ones.
        /// </summary>
        internal readonly CaseStatus LastStatus()
        {
            // NOTE: The statuses are ordered in reversed order (first item is the newest one)
            return this.Count > 0 ? this.Results[0]    // The very latest status
                                  : throw new HttpRequestException(Resources.HttpRequest_ERROR_NoLastStatus);
        }
    }
}