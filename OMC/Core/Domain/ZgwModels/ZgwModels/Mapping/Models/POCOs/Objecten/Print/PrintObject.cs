// © 2026, Worth Systems.

using System.Text.Json.Serialization;
using JetBrains.Annotations;
using ZgwModels.Mapping.Models.Interfaces;

namespace ZgwModels.Mapping.Models.POCOs.Objecten.Print
{
    /// <summary>
    /// The print job retrieved from "Objecten" Web API service.
    /// </summary>
    /// <remarks>
    ///   Written by the composing party (GZAC / Ritense) to hand a already-composed PDF to OMC for
    ///   printing and posting. OMC never renders the letter itself; it only fetches, forwards, and
    ///   registers what happened.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public struct PrintObject : IJsonSerializable
    {
        /// <summary>
        /// The record related to the <see cref="PrintObject"/>.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("record")]
        [JsonPropertyOrder(0)]
        public PrintRecord Record { get; [UsedImplicitly] set; }
    }
}
