// © 2023, Worth Systems.

using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;
using ZhvModels.Properties;

namespace ZhvModels.Mapping.Models.POCOs.OpenZaak
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
        [JsonPropertyName("count")]
        [JsonPropertyOrder(0)]
        public int Count { get; set; }

        /// <summary>
        /// The collection of:
        /// <inheritdoc cref="CaseStatus"/>
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("results")]
        [JsonPropertyOrder(1)]
        public List<CaseStatus> Results { get; set; } = [];

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
        public readonly bool WereNeverUpdated() => this.Count == 1;

        /// <summary>
        /// The newest <see cref="CaseStatus"/> from multiple ones.
        /// </summary>
        /// <exception cref="HttpRequestException"/>
        public readonly CaseStatus LastStatus()
        {
            // NOTE: The statuses are ordered in reversed order (first item is the newest one)
            return this.Count > 0 ? this.Results[0]  // The very latest status
                                  : throw new HttpRequestException(ZhvResources.HttpRequest_ERROR_NoLastStatus);
        }
    }
}