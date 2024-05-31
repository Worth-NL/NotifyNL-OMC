﻿// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// The ID of the digital address retrieved from "OpenKlant" Web service.
    /// </summary>
    /// <seealso cref="IJsonSerializable" />
    public struct DigitalAddressShort : IJsonSerializable
    {
        /// <summary>
        /// The UUID of the digital address.
        /// </summary>
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