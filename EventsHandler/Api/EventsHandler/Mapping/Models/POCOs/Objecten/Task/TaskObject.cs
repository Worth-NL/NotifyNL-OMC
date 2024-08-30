﻿// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;

namespace EventsHandler.Mapping.Models.POCOs.Objecten.Task
{
    /// <summary>
    /// The task retrieved from "Objecten" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct TaskObject : IJsonSerializable
    {
        /// <summary>
        /// The record related to the <see cref="TaskObject"/>.
        /// </summary>
        [JsonRequired]
        [JsonInclude]
        [JsonPropertyName("record")]
        [JsonPropertyOrder(0)]
        public Record Record { get; internal set; }
    }
}