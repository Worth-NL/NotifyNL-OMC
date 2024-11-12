// © 2023, Worth Systems.

using EventsHandler.Properties;
using EventsHandler.Services.Responding.Messages.Models.Details;
using EventsHandler.Services.Responding.Messages.Models.Details.Base;
using EventsHandler.Services.Responding.Results.Builder.Interface;
using EventsHandler.Services.Responding.Results.Enums;

namespace EventsHandler.Services.Responding.Results.Builder
{
    /// <summary>
    /// <inheritdoc cref="IDetailsBuilder"/>
    /// Building <see cref="ErrorDetails"/>.
    /// </summary>
    /// <seealso cref="IDetailsBuilder"/>
    public sealed class DetailsBuilder : IDetailsBuilder
    {
        private static readonly object s_lock = new();

        #region Cached details (objects)
        private static readonly Dictionary<Type, BaseEnhancedDetails> s_cachedEnhancedDetails = new()
        {
            { typeof(ErrorDetails), new ErrorDetails() },
            { typeof(InfoDetails), new InfoDetails() }
        };

        private static readonly Dictionary<Type, BaseSimpleDetails> s_cachedSimpleDetails = new()
        {
            { typeof(UnknownDetails), new UnknownDetails() }
        };
        #endregion

        #region Cached details (content)
        private static readonly Dictionary<Reasons, (string Message, string[] Reasons)> s_cachedDetailsContents = new()
        {
            {
                Reasons.InvalidJson,
                (Resources.Deserialization_ERROR_InvalidJson_Message,
                new[]
                {
                    Resources.Deserialization_ERROR_InvalidJson_Reason1
                })
            },
            {
                Reasons.MissingProperties_Notification,
                (Resources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Message,
                new[]
                {
                    Resources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Reason1,
                    Resources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Reason2,
                    Resources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Reason3
                })
            },
            {
                Reasons.InvalidProperties_Notification,
                (Resources.Deserialization_ERROR_NotDeserialized_Notification_Value_Message,
                new[]
                {
                    Resources.Deserialization_ERROR_NotDeserialized_Notification_Value_Reason1,
                    Resources.Deserialization_ERROR_NotDeserialized_Notification_Value_Reason2
                })
            },
            {
                Reasons.MissingProperties_Attributes,
                (Resources.Deserialization_ERROR_NotDeserialized_Attributes_Properties_Message,
                new[]
                {
                    Resources.Deserialization_INFO_NotDeserialized_Attributes_Properties_Reason1,
                    Resources.Deserialization_INFO_NotDeserialized_Attributes_Properties_Reason2,
                    Resources.Deserialization_INFO_NotDeserialized_Attributes_Properties_Reason3
                })
            },
            {
                Reasons.UnexpectedProperties_Notification,
                (Resources.Deserialization_ERROR_UnexpectedData_Notification_Message,
                new[]
                {
                    Resources.Deserialization_INFO_UnexpectedData_Notification_Reason1,
                    Resources.Deserialization_INFO_UnexpectedData_Notification_Reason2
                })
            },
            {
                Reasons.UnexpectedProperties_Attributes,
                (Resources.Deserialization_INFO_UnexpectedData_Attributes_Message,
                new[]
                {
                    Resources.Deserialization_INFO_UnexpectedData_Attributes_Reason1,
                    Resources.Deserialization_INFO_UnexpectedData_Attributes_Reason2
                })
            },
            {
                Reasons.HttpRequestError,
                (Resources.HttpRequest_ERROR_Message,
                new[]
                {
                    Resources.HttpRequest_ERROR_Reason1,
                    Resources.HttpRequest_ERROR_Reason2
                })
            },
            {
                Reasons.ValidationIssue,
                (Resources.Operation_ERROR_Unknown_ValidationIssue_Message, Array.Empty<string>())
            }
        };
        #endregion

        /// <inheritdoc cref="IDetailsBuilder.Get{TDetails}(Reasons, string)"/>
        TDetails IDetailsBuilder.Get<TDetails>(Reasons reason, string cases)
        {
            lock (s_lock)
            {
                var details = (TDetails)s_cachedEnhancedDetails[typeof(TDetails)];

                details.Message = s_cachedDetailsContents[reason].Message;
                details.Cases   = cases;
                details.Reasons = s_cachedDetailsContents[reason].Reasons;

                return details;
            }
        }

        /// <inheritdoc cref="IDetailsBuilder.Get{TDetails}(Reasons)"/>
        TDetails IDetailsBuilder.Get<TDetails>(Reasons reason)
        {
            lock (s_lock)
            {
                var details = (TDetails)s_cachedSimpleDetails[typeof(TDetails)];

                details.Message = s_cachedDetailsContents[reason].Message;

                return details;
            }
        }
    }
}