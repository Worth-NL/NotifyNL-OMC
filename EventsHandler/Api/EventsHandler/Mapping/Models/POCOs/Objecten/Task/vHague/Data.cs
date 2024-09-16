﻿// © 2024, Worth Systems.

using EventsHandler.Constants;
using EventsHandler.Mapping.Enums.Objecten;
using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.Objecten.Task.vHague
{
    /// <summary>
    /// The data related to the <see cref="Record"/> retrieved from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="CommonData"/>
    /// <seealso cref="IJsonSerializable"/>
    public struct Data : IJsonSerializable
    {
        /// <inheritdoc cref="CommonData.CaseUri"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("zaak")]
        [JsonPropertyOrder(0)]
        public Uri CaseUri { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <inheritdoc cref="CommonData.Title"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("title")]
        [JsonPropertyOrder(1)]
        public string Title { get; internal set; } = string.Empty;

        /// <inheritdoc cref="CommonData.Status"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("status")]
        [JsonPropertyOrder(2)]
        public TaskStatuses Status { get; internal set; }

        /// <inheritdoc cref="CommonData.ExpirationDate"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("verloopdatum")]
        [JsonPropertyOrder(3)]
        public DateTime ExpirationDate { get; internal set; }

        /// <inheritdoc cref="CommonData.Identification"/>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("identificatie")]
        [JsonPropertyOrder(4)]
        public Identification Identification { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Data"/> struct.
        /// </summary>
        public Data()
        {
        }
    }
}