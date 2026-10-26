// © 2026, Worth Systems.

using JetBrains.Annotations;
using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace ZgwModels.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// A page of klantcontacten from the "Klantinteracties" API of "OpenKlant", with only what OMC needs to tell
    /// whether it already wrote a klantcontact and what is still missing from it.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct KlantcontactResults : IJsonSerializable
    {
        /// <summary>
        /// The klantcontacten on this page.
        /// </summary>
        [JsonPropertyName("results")]
        [JsonPropertyOrder(0)]
        public List<ExistingKlantcontact> Results { get; [UsedImplicitly] set; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="KlantcontactResults"/> struct.
        /// </summary>
        public KlantcontactResults()
        {
        }
    }

    /// <summary>
    /// One klantcontact as listed by "OpenKlant".
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct ExistingKlantcontact : IJsonSerializable
    {
        /// <summary>
        /// The klantcontact's identifier.
        /// </summary>
        [JsonPropertyName("uuid")]
        [JsonPropertyOrder(0)]
        public Guid Id { get; [UsedImplicitly] set; } = Guid.Empty;

        /// <summary>
        /// Its generic key/value metadata.
        /// </summary>
        [JsonPropertyName("metadata")]
        [JsonPropertyOrder(1)]
        public Dictionary<string, string>? Metadata { get; [UsedImplicitly] set; }

        /// <summary>
        /// The actors linked to it.
        /// </summary>
        [JsonPropertyName("hadBetrokkenActoren")]
        [JsonPropertyOrder(2)]
        public List<LinkedResource>? Actors { get; [UsedImplicitly] set; }

        /// <summary>
        /// The bijlagen linked to it.
        /// </summary>
        [JsonPropertyName("omvatteBijlagen")]
        [JsonPropertyOrder(3)]
        public List<LinkedResource>? Attachments { get; [UsedImplicitly] set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExistingKlantcontact"/> struct.
        /// </summary>
        public ExistingKlantcontact()
        {
        }
    }

    /// <summary>
    /// A resource linked to a klantcontact, of which OMC only needs the identifier.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct LinkedResource : IJsonSerializable
    {
        /// <summary>
        /// The linked resource's identifier.
        /// </summary>
        [JsonPropertyName("uuid")]
        [JsonPropertyOrder(0)]
        public Guid Id { get; [UsedImplicitly] set; } = Guid.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedResource"/> struct.
        /// </summary>
        public LinkedResource()
        {
        }
    }
}
