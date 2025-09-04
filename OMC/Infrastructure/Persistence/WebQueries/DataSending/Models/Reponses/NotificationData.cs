// © 2025, Worth Systems.

using Newtonsoft.Json;
using Notify.Models;

namespace WebQueries.DataSending.Models.Reponses
{
    /// <summary>
    /// Contains details of notification from "Notify NL" Web API service.
    /// </summary>
    public struct NotificationData  // NOTE: "NotificationData" is restricted name of the model from "Notify.Models.Responses"
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("completed_at")]
        public string CompletedAt { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("email_address")]
        public string EmailAddress { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("line_1")]
        public string Line1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("line_2")]
        public string Line2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("line_3")]
        public string Line3 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("line_4")]
        public string Line4 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("line_5")]
        public string Line5 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("line_6")]
        public string Line6 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Postcode { get; set; }    

        /// <summary>
        /// 
        /// </summary>
        public string Postage { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("sent_at")]
        public string SentAt { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("created_by_name")]
        public string? CreatedByName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyTemplateResponse"/> struct.
        /// </summary>
        private NotificationData(bool isSuccess, Notification notification, string exception)
        {
            this.Id = notification.id;
            this.CompletedAt = notification.completedAt;
            this.CreatedAt = notification.createdAt;
            this.EmailAddress = notification.emailAddress;
            this.Body = notification.body;
            this.Subject = notification.subject;
            this.Line1 = notification.line1;
            this.Line2 = notification.line2;
            this.Line3 = notification.line3;
            this.Line4 = notification.line4;
            this.Line5 = notification.line5;
            this.Line6 = notification.line6;
            this.PhoneNumber = notification.phoneNumber;
            this.Postcode = notification.postcode;
            this.Postage = notification.postage;
            this.Reference = notification.reference;
            this.SentAt = notification.sentAt;
            this.Status = notification.status;
            this.Type = notification.type;
            this.CreatedByName = notification.createdByName;
            this.IsSuccess = isSuccess;
            this.Error = exception;
        }

        private NotificationData(bool isSuccess, string error)
        {
            Id = CompletedAt = CreatedAt = EmailAddress = Body = Subject = Line1 = Line2 = Line3 = Line4 = Line5 = Line6 = PhoneNumber = Postcode = Postage = Reference = SentAt = Status = Type = CreatedByName = String.Empty;
            IsSuccess = isSuccess;
            Error = error;
        }

        /// <summary>
        /// Success result.
        /// </summary>
        public static NotificationData Success(Notification notification)
            => new(true, notification, string.Empty);

        /// <summary>
        /// Failure result.
        /// </summary>
        public static NotificationData Failure(string error)
            => new(false, error);
    }
}