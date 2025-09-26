// © 2023, Worth Systems.

using Common.Constants;
using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.OpenZaak
{
    /// <summary>
    /// The case retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Case : IJsonSerializable
    {
        /// <summary>
        /// The reference to the <see cref="Case"/> in <see cref="System.Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("url")]
        [JsonPropertyOrder(0)]
        public Uri Uri { get; set; } = CommonValues.Default.Models.EmptyUri;

        /// <summary>
        /// The identification of the <see cref="Case"/> in the following format:
        /// <code>
        /// ZAAK-2023-0000000010
        /// </code>
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("identificatie")]
        [JsonPropertyOrder(1)]
        public string Identification { get; set; } = string.Empty;

        /// <summary>
        /// The name of the <see cref="Case"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("omschrijving")]
        [JsonPropertyOrder(2)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The type of the <see cref="Case"/> in <see cref="System.Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("zaaktype")]
        [JsonPropertyOrder(3)]
        public Uri CaseTypeUri { get; set; } = CommonValues.Default.Models.EmptyUri;

        /// <summary>
        /// The date when the <see cref="Case"/> was registered.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("registratiedatum")]
        [JsonPropertyOrder(4)]
        public DateOnly RegistrationDate { get; set; }

        /// <summary>
        /// The result of the <see cref="Case"/> in <see cref="System.Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonPropertyName("resultaat")]
        [JsonPropertyOrder(5)]
        public Uri ResultUri { get; set; } = CommonValues.Default.Models.EmptyUri;

        /// <summary>
        /// Contains expanded objects requested via the `expand` query parameter.
        /// </summary>
        [JsonPropertyName("_expand")]
        [JsonPropertyOrder(6)]
        public ExpandedResult? Expanded { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Case"/> struct.
        /// </summary>
        public Case()
        {
        }

        /// <summary>
        /// Represents the `_expand` section of the API response.
        /// </summary>
        public struct ExpandedResult
        {
            /// <summary>
            /// 
            /// </summary>
            [JsonPropertyName("resultaat")]
            public Result Result { get; set; }
        }

        /// <summary>
        /// Represents the expanded `result` object.
        /// </summary>
        public struct Result
        {
            /// <summary>
            /// 
            /// </summary>
            [JsonPropertyName("resultaattype")]
            public Uri ResultType { get; set; } = CommonValues.Default.Models.EmptyUri;

            /// <summary>
            /// 
            /// </summary>
            public Result()
            {
            }
        }
    }
}