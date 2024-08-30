﻿// © 2023, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak
{
    /// <summary>
    /// The case retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Case : IJsonSerializable
    {
        /// <summary>
        /// The identification of the <see cref="Case"/> in the following format:
        /// <code>
        /// ZAAK-2023-0000000010
        /// </code>
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("identificatie")]
        [JsonPropertyOrder(0)]
        public string Identification { get; internal set; } = string.Empty;

        /// <summary>
        /// The name of the <see cref="Case"/>.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("omschrijving")]
        [JsonPropertyOrder(1)]
        public string Name { get; internal set; } = string.Empty;

        /// <summary>
        /// The type of the <see cref="Case"/> in <see cref="Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("zaaktype")]
        [JsonPropertyOrder(2)]
        public Uri CaseTypeUri { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// The date when the <see cref="Case"/> was registered.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("registratiedatum")]
        [JsonPropertyOrder(3)]
        public DateOnly RegistrationDate { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Case"/> struct.
        /// </summary>
        public Case()
        {
        }
    }
}