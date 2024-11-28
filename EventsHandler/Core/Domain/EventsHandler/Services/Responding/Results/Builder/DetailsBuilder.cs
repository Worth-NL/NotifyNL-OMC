﻿// © 2023, Worth Systems.

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
                (ApiResources.Deserialization_ERROR_InvalidJson_Message,
                [
                    ApiResources.Deserialization_ERROR_InvalidJson_Reason1
                ])
            },
            {
                Reasons.MissingProperties_Notification,
                (ApiResources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Message,
                [
                    ApiResources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Reason1,
                    ApiResources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Reason2,
                    ApiResources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Reason3
                ])
            },
            {
                Reasons.InvalidProperties_Notification,
                (ApiResources.Deserialization_ERROR_NotDeserialized_Notification_Value_Message,
                [
                    ApiResources.Deserialization_ERROR_NotDeserialized_Notification_Value_Reason1,
                    ApiResources.Deserialization_ERROR_NotDeserialized_Notification_Value_Reason2
                ])
            },
            {
                Reasons.MissingProperties_Attributes,
                (ApiResources.Deserialization_ERROR_NotDeserialized_Attributes_Properties_Message,
                [
                    ApiResources.Deserialization_INFO_NotDeserialized_Attributes_Properties_Reason1,
                    ApiResources.Deserialization_INFO_NotDeserialized_Attributes_Properties_Reason2,
                    ApiResources.Deserialization_INFO_NotDeserialized_Attributes_Properties_Reason3
                ])
            },
            {
                Reasons.UnexpectedProperties_Notification,
                (ApiResources.Deserialization_ERROR_UnexpectedData_Notification_Message,
                [
                    ApiResources.Deserialization_INFO_UnexpectedData_Notification_Reason1,
                    ApiResources.Deserialization_INFO_UnexpectedData_Notification_Reason2
                ])
            },
            {
                Reasons.UnexpectedProperties_Attributes,
                (ApiResources.Deserialization_INFO_UnexpectedData_Attributes_Message,
                [
                    ApiResources.Deserialization_INFO_UnexpectedData_Attributes_Reason1,
                    ApiResources.Deserialization_INFO_UnexpectedData_Attributes_Reason2
                ])
            },
            {
                Reasons.HttpRequestError,
                (ApiResources.HttpRequest_ERROR_Message,
                [
                    ApiResources.HttpRequest_ERROR_Reason1,
                    ApiResources.HttpRequest_ERROR_Reason2
                ])
            },
            {
                Reasons.ValidationIssue,
                (ApiResources.Operation_ERROR_Unknown_ValidationIssue_Message, [])
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