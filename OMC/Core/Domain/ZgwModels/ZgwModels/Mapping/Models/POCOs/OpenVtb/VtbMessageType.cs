using Common.Constants;
using System.Text.Json.Serialization;
using ZgwModels.Mapping.Models.Interfaces;

namespace ZgwModels.Mapping.Models.POCOs.OpenVtb
{
    /// <summary>
    /// Represents a message type (BerichtType) retrieved from the OpenVTB API.
    /// </summary>
    /// <remarks>
    ///   Since Open VTB 0.2.0 a <see cref="VtbMessage"/> only links to its type; whether it may go to the
    ///   Berichtenbox, and the code Logius knows it by, are read from here.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public struct VtbMessageType : IJsonSerializable
    {
        /// <summary>
        /// The unique URL of the BerichtType within this API.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("url")]
        [JsonPropertyOrder(0)]
        public Uri Url { get; set; } = CommonValues.Default.Models.EmptyUri;

        /// <summary>
        /// The Uniform Resource Name of the BerichtType.
        /// </summary>
        [JsonPropertyName("urn")]
        [JsonPropertyOrder(1)]
        public string Urn { get; set; } = string.Empty;

        /// <summary>
        /// Unique identifier (UUID4) for the BerichtType.
        /// </summary>
        [JsonPropertyName("uuid")]
        [JsonPropertyOrder(2)]
        public string Uuid { get; set; } = string.Empty;

        /// <summary>
        /// The action to be performed by the assigned person or company.
        /// </summary>
        [JsonPropertyName("handelingsPerspectief")]
        [JsonPropertyOrder(3)]
        public string? ActionsPerspective { get; set; }

        /// <summary>
        /// Indicates whether messages of this type are suitable for publication in the MijnOverheid Berichtenbox.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("mijnOverheidBerichtenbox")]
        [JsonPropertyOrder(4)]
        public bool IsInMyGovernmentMessageBox { get; set; }

        /// <summary>
        /// Code for technical identification of the message type/origin in the MijnOverheid Berichtenbox (max 8 characters).
        /// </summary>
        [JsonPropertyName("mijnOverheidBerichtenboxType")]
        [JsonPropertyOrder(5)]
        public string? MessageBoxType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VtbMessageType"/> struct.
        /// </summary>
        public VtbMessageType()
        {
        }
    }
}
