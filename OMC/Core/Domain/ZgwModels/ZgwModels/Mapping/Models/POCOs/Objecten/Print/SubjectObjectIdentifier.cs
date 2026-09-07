// © 2026, Worth Systems.

using System.Text.Json.Serialization;
using JetBrains.Annotations;
using ZgwModels.Mapping.Models.Interfaces;

namespace ZgwModels.Mapping.Models.POCOs.Objecten.Print
{
    /// <summary>
    /// The "onderwerpobjectidentificator" supplied on a <see cref="PrintData"/>, identifying the object
    /// (normally a zaak) the letter is about.
    /// </summary>
    /// <remarks>
    ///   Everywhere else in OMC these three "code..." values come from configuration, because the flow
    ///   itself knows what it is registering against. Here the writing party knows better, so the payload
    ///   takes precedence and configuration is only the fallback for whichever fields are left blank.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public struct SubjectObjectIdentifier : IJsonSerializable
    {
        /// <summary>
        /// The identifier of the object the letter is about - typically the zaak UUID.
        /// </summary>
        [JsonPropertyName("objectId")]
        [JsonPropertyOrder(0)]
        public string ObjectId { get; [UsedImplicitly] set; } = string.Empty;

        /// <summary>
        /// The type of the object, for example "zaak".
        /// </summary>
        [JsonPropertyName("codeObjecttype")]
        [JsonPropertyOrder(1)]
        public string CodeObjectType { get; [UsedImplicitly] set; } = string.Empty;

        /// <summary>
        /// The register the object lives in, for example "openzaak".
        /// </summary>
        [JsonPropertyName("codeRegister")]
        [JsonPropertyOrder(2)]
        public string CodeRegister { get; [UsedImplicitly] set; } = string.Empty;

        /// <summary>
        /// The kind of the <see cref="ObjectId"/>, for example "uuid".
        /// </summary>
        [JsonPropertyName("codeSoortObjectId")]
        [JsonPropertyOrder(3)]
        public string CodeSoortObjectId { get; [UsedImplicitly] set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubjectObjectIdentifier"/> struct.
        /// </summary>
        public SubjectObjectIdentifier()
        {
        }
    }
}
