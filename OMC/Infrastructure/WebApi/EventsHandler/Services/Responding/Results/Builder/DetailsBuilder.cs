// © 2023, Worth Systems.

using Common.Models.Messages.Details;
using Common.Models.Messages.Details.Base;
using EventsHandler.Properties;
using EventsHandler.Services.Responding.Enums;
using EventsHandler.Services.Responding.Results.Builder.Interface;
using ZhvModels.Properties;

namespace EventsHandler.Services.Responding.Results.Builder
{
    /// <summary>
    /// Builds structured details objects used in API responses.
    /// </summary>
    internal sealed class DetailsBuilder : IDetailsBuilder
    {
        private static readonly object s_lock = new();

        private static readonly Dictionary<Reasons, (string Message, string[] Reasons)> s_cachedDetailsContents = new()
        {
            {
                Reasons.InvalidJson,
                (ZhvResources.Deserialization_ERROR_InvalidJson_Message,
                [
                    ZhvResources.Deserialization_ERROR_InvalidJson_Reason1
                ])
            },
            {
                Reasons.MissingProperties_Notification,
                (ZhvResources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Message,
                [
                    ZhvResources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Reason1,
                    ZhvResources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Reason2,
                    ZhvResources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Reason3
                ])
            },
            {
                Reasons.InvalidProperties_Notification,
                (ZhvResources.Deserialization_ERROR_NotDeserialized_Notification_Value_Message,
                [
                    ZhvResources.Deserialization_ERROR_NotDeserialized_Notification_Value_Reason1,
                    ZhvResources.Deserialization_ERROR_NotDeserialized_Notification_Value_Reason2
                ])
            },
            {
                Reasons.MissingProperties_Attributes,
                (ZhvResources.Deserialization_ERROR_NotDeserialized_Attributes_Properties_Message,
                [
                    ZhvResources.Deserialization_INFO_NotDeserialized_Attributes_Properties_Reason1,
                    ZhvResources.Deserialization_INFO_NotDeserialized_Attributes_Properties_Reason2,
                    ZhvResources.Deserialization_INFO_NotDeserialized_Attributes_Properties_Reason3
                ])
            },
            {
                Reasons.UnexpectedProperties_Notification,
                (ZhvResources.Deserialization_ERROR_UnexpectedData_Notification_Message,
                [
                    ZhvResources.Deserialization_INFO_UnexpectedData_Notification_Reason1,
                    ZhvResources.Deserialization_INFO_UnexpectedData_Notification_Reason2
                ])
            },
            {
                Reasons.UnexpectedProperties_Attributes,
                (ZhvResources.Deserialization_INFO_UnexpectedData_Attributes_Message,
                [
                    ZhvResources.Deserialization_INFO_UnexpectedData_Attributes_Reason1,
                    ZhvResources.Deserialization_INFO_UnexpectedData_Attributes_Reason2
                ])
            },
            {
                Reasons.HttpRequestError,
                (ZhvResources.HttpRequest_ERROR_Message,
                [
                    ZhvResources.HttpRequest_ERROR_Reason1,
                    ZhvResources.HttpRequest_ERROR_Reason2
                ])
            },
            {
                Reasons.ValidationIssue,
                (ApiResources.Operation_ERROR_Unknown_ValidationIssue_Message, [])
            }
        };

        TDetails IDetailsBuilder.Get<TDetails>(Reasons reason, string cases)
        {
            lock (s_lock)
            {
                var details = Activator.CreateInstance<TDetails>();

                details.Message = s_cachedDetailsContents[reason].Message;
                details.Cases = cases;
                details.Reasons = s_cachedDetailsContents[reason].Reasons;

                return details;
            }
        }

        TDetails IDetailsBuilder.Get<TDetails>(Reasons reason)
        {
            lock (s_lock)
            {
                var details = Activator.CreateInstance<TDetails>();

                details.Message = s_cachedDetailsContents[reason].Message;

                return details;
            }
        }
    }
}