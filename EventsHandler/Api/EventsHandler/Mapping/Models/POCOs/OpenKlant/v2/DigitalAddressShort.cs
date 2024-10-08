﻿// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// The ID of the digital address retrieved from "OpenKlant" Web API service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (2.0) Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IJsonSerializable" />
    public struct DigitalAddressShort : IJsonSerializable
    {
        /// <summary>
        /// The UUID / GUID of the digital address.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("uuid")]
        [JsonPropertyOrder(0)]
        public Guid Id { get; internal set; } = Guid.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalAddressShort"/> struct.
        /// </summary>
        public DigitalAddressShort()
        {
        }
    }
}