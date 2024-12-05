// © 2023, Worth Systems.

using Common.Constants;
using Common.Extensions;
using Common.Models.Messages.Details;
using Common.Models.Messages.Details.Base;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ZhvModels.Mapping.Enums.NotificatieApi;
using ZhvModels.Mapping.Helpers;
using ZhvModels.Mapping.Models.Interfaces;

namespace ZhvModels.Mapping.Models.POCOs.NotificatieApi
{
    /// <summary>
    /// The main notification retrieved as a callback event from "OpenNotificaties" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct NotificationEvent : IJsonSerializable  // NOTE: This model is used in endpoints + Swagger UI examples and it must be public
    {
        #region Metadata
        private static readonly object s_lock = new();
        private static PropertiesMetadata? s_properties;

        /// <summary>
        /// Gets metadata of all (useful) public instance properties from the <see cref="NotificationEvent"/> POCO model.
        /// </summary>
        public readonly PropertiesMetadata Properties
        {
            get
            {
                if (s_properties == null)
                {
                    // Critical Section
                    lock (s_lock)
                    {
                        // Metadata initialization
                        s_properties ??= new PropertiesMetadata(this,
                            nameof(this.Attributes), nameof(this.Orphans));
                    }
                }

                return s_properties;
            }
        }
        #endregion

        /// <inheritdoc cref="Actions"/>
        [Required]                   // API model validation
        [JsonRequired]               // Property is required and must be mapped
        [JsonPropertyName("actie")]  // Specific JSON name to be mapped into property. In this case: "Dutch => English"
        [JsonPropertyOrder(0)]       // Specific order of properties - useful when displaying serialized objects
        public Actions Action { get; set; }

        /// <inheritdoc cref="Channels"/>
        [Required]
        [JsonRequired]
        [JsonPropertyName("kanaal")]
        [JsonPropertyOrder(1)]
        public Channels Channel { get; set; }

        /// <inheritdoc cref="Resources"/>
        [Required]
        [JsonRequired]
        [JsonPropertyName("resource")]
        [JsonPropertyOrder(2)]
        public Resources Resource { get; set; }

        /// <summary>
        /// The generic attributes of the <see cref="NotificationEvent"/>.
        /// </summary>
        /// <remarks>
        /// The source Web API service specifies attributes.
        /// </remarks>
        [Required]
        [JsonRequired]
        [JsonPropertyName("kenmerken")]
        [JsonPropertyOrder(3)]
        public EventAttributes Attributes { get; set; }

        /// <summary>
        /// The reference to the domain object in <see cref="Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [Required]
        [JsonRequired]
        [JsonPropertyName("hoofdObject")]
        [JsonPropertyOrder(4)]
        public Uri MainObjectUri { get; set; } = CommonValues.Default.Models.EmptyUri;

        /// <summary>
        /// The reference to the resource in <see cref="Uri"/> format:
        /// <code>
        /// http(s)://Domain/ApiEndpoint/[UUID]
        /// </code>
        /// </summary>
        [Required]
        [JsonRequired]
        [JsonPropertyName("resourceUrl")]
        [JsonPropertyOrder(5)]
        public Uri ResourceUri { get; set; } = CommonValues.Default.Models.EmptyUri;

        /// <summary>
        /// The date and time when the action took place.
        /// </summary>
        [Required]
        [JsonRequired]
        [JsonPropertyName("aanmaakdatum")]
        [JsonPropertyOrder(6)]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// The JSON properties that couldn't be matched with properties of this specific POCO model => The orphans.
        /// </summary>
        [JsonExtensionData]  // Aggregate all JSON properties that couldn't be matched with this model
        [JsonPropertyOrder(7)]
        public Dictionary<string, object> Orphans { get; set; } = [];

        #region public Properties
        /// <summary>
        /// A specific details encountered during <see cref="NotificationEvent"/> validation.
        /// <para>
        ///   This property is meant to be used only for <see langword="internal"/> purposes.
        /// </para>
        /// </summary>
        [JsonIgnore]
        public BaseEnhancedDetails Details { get; set; } = InfoDetails.Empty;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationEvent"/> struct.
        /// </summary>
        public NotificationEvent()
        {
        }

        #region Validation
        /// <summary>
        /// Checks whether the <see cref="NotificationEvent"/> model wasn't
        /// initialized (and it has default values) for required properties.
        /// </summary>
        public readonly bool IsInvalidEvent(out int[] invalidPropertiesIndices)
        {
            bool[] validatedProperties =
            [
                this.Action        == Actions.Unknown,
                this.Channel       == Channels.Unknown,
                this.Resource      == Resources.Unknown,
                this.MainObjectUri == CommonValues.Default.Models.EmptyUri,
                this.ResourceUri   == CommonValues.Default.Models.EmptyUri
            ];

            List<int>? invalidIndices = null;

            for (int index = 0; index < validatedProperties.Length; index++)
            {
                if (validatedProperties[index])  // Is invalid
                {
                    (invalidIndices ??= []).Add(index);
                }
            }

            bool arePropertiesInvalid = invalidIndices.HasAny();

            invalidPropertiesIndices = arePropertiesInvalid
                ? invalidIndices!.ToArray()
                : [];

            return arePropertiesInvalid;
        }
        #endregion
    }
}