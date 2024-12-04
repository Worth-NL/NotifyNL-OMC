// © 2023, Worth Systems.

using NotificatieApi;
using NotificatieApi;
using NotificatieApi;
using NotificatieApi;
using NotificatieApi;
using NotificatieApi;
using NotificatieApi;

namespace EventsHandler.Services.Responding.Results.Enums
{
    /// <summary>
    /// The specific purpose of result details.
    /// </summary>
    internal enum Reasons
    {
        // ReSharper disable InconsistentNaming => Allow names with underscores

        /// <summary>
        /// The details handling the remaining cases of validation issues.
        /// </summary>
        ValidationIssue = 0,

        /// <summary>
        /// Details for invalid JSON payload.
        /// <para>
        ///   To be used with the validation message including syntax violation hints.
        /// </para>
        /// </summary>
        InvalidJson = 1,

        /// <summary>
        /// Details for missing data in <see cref="ZhvModels.Mapping.Models.POCOs.NotificatieApi.NotificationEvent"/> due to failed deserialization of its properties.
        /// <para>
        ///   To be used with the names of missing JSON parameters.
        /// </para>
        /// </summary>
        MissingProperties_Notification = 2,

        /// <summary>
        /// Details for unexpected value in <see cref="ZhvModels.Mapping.Models.POCOs.NotificatieApi.NotificationEvent"/> due to unsupported data type, format or range.
        /// <para>
        ///   To be used with the unexpected value.
        /// </para>
        /// </summary>
        InvalidProperties_Notification = 3,

        /// <summary>
        /// Details for missing data in <see cref="ZhvModels.Mapping.Models.POCOs.NotificatieApi.EventAttributes"/> due to failed deserialization of its properties.
        /// <para>
        ///   To be used with the names of missing JSON parameters.
        /// </para>
        /// </summary>
        MissingProperties_Attributes = 4,

        /// <summary>
        /// Details for unexpected data in <see cref="ZhvModels.Mapping.Models.POCOs.NotificatieApi.NotificationEvent"/>.
        /// <para>
        ///   To be used with the JSON properties which couldn't be matched with the existing <see cref="ZhvModels.Mapping.Models.POCOs.NotificatieApi.NotificationEvent"/> model.
        /// </para>
        /// </summary>
        UnexpectedProperties_Notification = 5,

        /// <summary>
        /// Details for unexpected data in <see cref="ZhvModels.Mapping.Models.POCOs.NotificatieApi.EventAttributes"/>.
        /// <para>
        ///   To be used with the JSON properties which couldn't be matched with the existing <see cref="ZhvModels.Mapping.Models.POCOs.NotificatieApi.EventAttributes"/> model.
        /// </para>
        /// </summary>
        UnexpectedProperties_Attributes = 6,

        /// <summary>
        /// Details for the case when HTTP Request failed.
        /// <para>
        ///   To be used with the HTTP Request error message.
        /// </para>
        /// </summary>
        HttpRequestError = 7
    }
}