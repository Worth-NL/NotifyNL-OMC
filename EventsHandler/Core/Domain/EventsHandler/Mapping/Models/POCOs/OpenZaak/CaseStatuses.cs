// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using EventsHandler.Properties;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak
{
    /// <summary>
    /// The statuses of the <see cref="Case"/> retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CaseStatuses : IJsonSerializable
    {
        /// <summary>
        /// The number of received results.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("count")]
        [JsonPropertyOrder(0)]
        public int Count { get; internal set; }

        /// <summary>
        /// The collection of:
        /// <inheritdoc cref="CaseStatus"/>
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("results")]
        [JsonPropertyOrder(1)]
        public List<CaseStatus> Results { get; internal set; } = [];

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
        /// The newest <see cref="CaseStatus"/> from multiple ones.
        /// </summary>
        /// <exception cref="HttpRequestException"/>
        internal readonly CaseStatus LastStatus()
        {
            // NOTE: The statuses are ordered in reversed order (first item is the newest one)
            return this.Count > 0 ? this.Results[0]  // The very latest status
                                  : throw new HttpRequestException(ApiResources.HttpRequest_ERROR_NoLastStatus);
        }
    }
}