﻿// © 2024, Worth Systems.

using Common.Constants;
using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.OpenZaak.Decision
{
    /// <summary>
    /// The decision related to the <see cref="DecisionResource"/> retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct Decision : IJsonSerializable
    {
        /// <summary>
        /// The identification of the <see cref="Decision"/> in the following format:
        /// <code>
        /// BESLUIT-2019-0000000002
        /// </code>
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("identificatie")]
        [JsonPropertyOrder(0)]
        public string Identification { get; set; } = string.Empty;

        /// <summary>
        /// The type of the <see cref="Decision"/> in <see cref="Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("besluittype")]
        [JsonPropertyOrder(1)]
        public Uri TypeUri { get; set; } = CommonValues.Default.Models.EmptyUri;

        /// <summary>
        /// The reference to the <see cref="Case"/> in <see cref="Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("zaak")]
        [JsonPropertyOrder(2)]
        public Uri CaseUri { get; set; } = CommonValues.Default.Models.EmptyUri;

        /// <summary>
        /// The date when the <see cref="Decision"/> was issued.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("datum")]
        [JsonPropertyOrder(3)]
        public DateOnly Date { get; set; }

        /// <summary>
        /// The explanation of the <see cref="Decision"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("toelichting")]
        [JsonPropertyOrder(4)]
        public string Explanation { get; set; } = string.Empty;

        /// <summary>
        /// The name of the institution issuing this <see cref="Decision"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("bestuursorgaan")]
        [JsonPropertyOrder(5)]
        public string GoverningBody { get; set; } = string.Empty;

        /// <summary>
        /// The date from which the <see cref="Decision"/> starts to operate (begins).
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("ingangsdatum")]
        [JsonPropertyOrder(6)]
        public DateOnly EffectiveDate { get; set; }

        /// <summary>
        /// The date after which the <see cref="Decision"/> will expire (ends).
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("vervaldatum")]
        [JsonPropertyOrder(7)]
        public DateOnly ExpirationDate { get; set; }

        /// <summary>
        /// The reason explaining the <see cref="Decision"/> expiration.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("vervalreden")]
        [JsonPropertyOrder(8)]
        public string ExpirationReason { get; set; } = string.Empty;

        /// <summary>
        /// The date when the <see cref="Decision"/> is meant to be published.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("publicatiedatum")]
        [JsonPropertyOrder(9)]
        public DateOnly PublicationDate { get; set; }

        /// <summary>
        /// The date when the <see cref="Decision"/> is meant to be shipped.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("verzenddatum")]
        [JsonPropertyOrder(10)]
        public DateOnly ShippingDate { get; set; }

        /// <summary>
        /// The date up to which party (e.g., citizen, or organization) can appeal the <see cref="Decision"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("uiterlijkeReactiedatum")]
        [JsonPropertyOrder(11)]
        public DateOnly ResponseDate { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decision"/> struct.
        /// </summary>
        public Decision()
        {
        }
    }
}