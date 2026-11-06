// © 2023, Worth Systems.

using Common.Models.Messages.Details;
using Common.Models.Messages.Details.Base;
using EventsHandler.Properties;
using EventsHandler.Services.Responding.Enums;
using EventsHandler.Services.Responding.Results.Builder.Interface;
using ZgwModels.Properties;

namespace EventsHandler.Services.Responding.Results.Builder
{
    /// <summary>
    /// <inheritdoc cref="IDetailsBuilder"/>
    /// <para>
    ///   Building:
    /// 
    ///   <list type="bullet">
    ///     <item><see cref="ErrorDetails"/></item>
    ///     <item><see cref="InfoDetails"/></item>
    ///     <item><see cref="UnknownDetails"/></item>
    ///   </list>
    /// </para>
    /// </summary>
    /// <seealso cref="IDetailsBuilder"/>
    internal sealed class DetailsBuilder : IDetailsBuilder
    {
        #region Cached details (content)
        private static readonly Dictionary<Reasons, (string Message, string[] Reasons)> s_cachedDetailsContents = new()
        {
            {
                Reasons.InvalidJson,
                (ZgwResources.Deserialization_ERROR_InvalidJson_Message,
                [
                    ZgwResources.Deserialization_ERROR_InvalidJson_Reason1
                ])
            },
            {
                Reasons.MissingProperties_Notification,
                (ZgwResources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Message,
                [
                    ZgwResources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Reason1,
                    ZgwResources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Reason2,
                    ZgwResources.Deserialization_ERROR_NotDeserialized_Notification_Properties_Reason3
                ])
            },
            {
                Reasons.InvalidProperties_Notification,
                (ZgwResources.Deserialization_ERROR_NotDeserialized_Notification_Value_Message,
                [
                    ZgwResources.Deserialization_ERROR_NotDeserialized_Notification_Value_Reason1,
                    ZgwResources.Deserialization_ERROR_NotDeserialized_Notification_Value_Reason2
                ])
            },
            {
                Reasons.MissingProperties_Attributes,
                (ZgwResources.Deserialization_ERROR_NotDeserialized_Attributes_Properties_Message,
                [
                    ZgwResources.Deserialization_INFO_NotDeserialized_Attributes_Properties_Reason1,
                    ZgwResources.Deserialization_INFO_NotDeserialized_Attributes_Properties_Reason2,
                    ZgwResources.Deserialization_INFO_NotDeserialized_Attributes_Properties_Reason3
                ])
            },
            {
                Reasons.UnexpectedProperties_Notification,
                (ZgwResources.Deserialization_ERROR_UnexpectedData_Notification_Message,
                [
                    ZgwResources.Deserialization_INFO_UnexpectedData_Notification_Reason1,
                    ZgwResources.Deserialization_INFO_UnexpectedData_Notification_Reason2
                ])
            },
            {
                Reasons.UnexpectedProperties_Attributes,
                (ZgwResources.Deserialization_INFO_UnexpectedData_Attributes_Message,
                [
                    ZgwResources.Deserialization_INFO_UnexpectedData_Attributes_Reason1,
                    ZgwResources.Deserialization_INFO_UnexpectedData_Attributes_Reason2
                ])
            },
            {
                Reasons.HttpRequestError,
                (ZgwResources.HttpRequest_ERROR_Message,
                [
                    ZgwResources.HttpRequest_ERROR_Reason1,
                    ZgwResources.HttpRequest_ERROR_Reason2
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
            // NOTE: A new object on every call - the builder is a singleton, and a shared object let one response pick up the details of another
            return new TDetails
            {
                Message = s_cachedDetailsContents[reason].Message,
                Cases   = cases,
                Reasons = s_cachedDetailsContents[reason].Reasons
            };
        }

        /// <inheritdoc cref="IDetailsBuilder.Get{TDetails}(Reasons)"/>
        TDetails IDetailsBuilder.Get<TDetails>(Reasons reason)
        {
            return new TDetails
            {
                Message = s_cachedDetailsContents[reason].Message
            };
        }
    }
}