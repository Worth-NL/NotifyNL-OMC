// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums.NotificatieApi;
using EventsHandler.Behaviors.Mapping.Helpers;
using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Constants;
using EventsHandler.Extensions;
using EventsHandler.Services.Responding.Messages.Models.Details;
using EventsHandler.Services.Responding.Messages.Models.Details.Base;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi
{
    /// <summary>
    /// The main notification retrieved as a callback event from "OpenNotificaties" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct NotificationEvent : IJsonSerializable
    {
        #region Metadata
        private static readonly object s_lock = new();
        private static PropertiesMetadata? s_properties;

        /// <summary>
        /// Gets metadata of all (useful) public instance properties from the <see cref="NotificationEvent"/> POCO model.
        /// </summary>
        internal readonly PropertiesMetadata Properties
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
        [JsonInclude]                // Allow JsonSerializer to save into properties with non-public setter
        [JsonRequired]               // Property is required and must be mapped
        [JsonPropertyName("actie")]  // Specific JSON name to be mapped into property. In this case: "Dutch => English"
        [JsonPropertyOrder(0)]       // Specific order of properties - useful when displaying serialized objects
        public Actions Action { get; internal set; }

        /// <inheritdoc cref="Channels"/>
        [Required]
        [JsonInclude]
        [JsonRequired]
        [JsonPropertyName("kanaal")]
        [JsonPropertyOrder(1)]
        public Channels Channel { get; internal set; }

        /// <inheritdoc cref="Resources"/>
        [Required]
        [JsonInclude]
        [JsonRequired]
        [JsonPropertyName("resource")]
        [JsonPropertyOrder(2)]
        public Resources Resource { get; internal set; }

        /// <summary>
        /// Mapping of attributes (key/value) of the notification. The publishing API specifies the allowed attributes.
        /// </summary>
        [Required]
        [JsonInclude]
        [JsonRequired]
        [JsonPropertyName("kenmerken")]
        [JsonPropertyOrder(3)]
        public EventAttributes Attributes { get; internal set; }

        /// <summary>
        /// URL reference to the main publishing API object related to the resource.
        /// </summary>
        [Required]
        [JsonInclude]
        [JsonRequired]
        [JsonPropertyName("hoofdObject")]
        [JsonPropertyOrder(4)]
        public Uri MainObject { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// URL reference to the resource publishing API.
        /// </summary>
        [Required]
        [JsonInclude]
        [JsonRequired]
        [JsonPropertyName("resourceUrl")]
        [JsonPropertyOrder(5)]
        public Uri ResourceUrl { get; internal set; } = DefaultValues.Models.EmptyUri;

        /// <summary>
        /// Date and time the action took place.
        /// </summary>
        [Required]
        [JsonInclude]
        [JsonRequired]
        [JsonPropertyName("aanmaakdatum")]
        [JsonPropertyOrder(6)]
        public DateTime CreateDate { get; internal set; }

        /// <summary>
        /// The JSON properties that couldn't be matched with properties of this specific POCO model => The orphans.
        /// </summary>
        [JsonInclude]
        [JsonExtensionData]     // Aggregate all JSON properties that couldn't be matched with this model
        [JsonPropertyOrder(7)]
        public Dictionary<string, object> Orphans { get; internal set; } = new();

        #region Internal Properties
        /// <summary>
        /// A specific details encountered during <see cref="NotificationEvent"/> validation.
        /// <para>
        ///   This property is meant to be used only for <see langword="internal"/> purposes.
        /// </para>
        /// </summary>
        [JsonIgnore]
        internal BaseEnhancedDetails Details { get; set; } = InfoDetails.Empty;
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
        internal readonly bool IsInvalidEvent(out int[] invalidPropertiesIndices)
        {
            bool[] validatedProperties =
            {
                this.Action      == Actions.Unknown,
                this.Channel     == Channels.Unknown,
                this.Resource    == Resources.Unknown,
                this.MainObject  == DefaultValues.Models.EmptyUri,
                this.ResourceUrl == DefaultValues.Models.EmptyUri
            };

            List<int>? invalidIndices = null;

            for (int index = 0; index < validatedProperties.Length; index++)
            {
                if (validatedProperties[index])  // Is invalid
                {
                    (invalidIndices ??= new List<int>()).Add(index);
                }
            }

            bool arePropertiesInvalid = invalidIndices.HasAny();

            invalidPropertiesIndices = arePropertiesInvalid
                ? invalidIndices!.ToArray()
                : Array.Empty<int>();

            return arePropertiesInvalid;
        }
        #endregion
    }
}