﻿// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// The identification of the party (e.g., citizen, organization) retrieved from "OpenKlant" Web service.
    /// </summary>
    /// <seealso cref="IJsonSerializable" />
    public struct PartyIdentification : IJsonSerializable
    {
        /// <inheritdoc cref="PartyDetails"/>
        [JsonInclude]
        [JsonPropertyName("contactnaam")]
        [JsonPropertyOrder(0)]
        public PartyDetails Details { get; internal set; }
    }
}