using System.Text.Json.Serialization;
using ZhvModels.Mapping.Models.Interfaces;

namespace WebQueries.KTO.Models
{
    /// <summary>
    /// Represents the root object containing customer data.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct KtoCustomerObject : IJsonSerializable
    {
        /// <summary>
        /// The email address retrieved from the KlantAPI.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("emailadres")]
        [JsonPropertyOrder(1)]
        public string Emailadres { get; init; }

        /// <summary>
        /// The transaction date when the final status was created in the ZaakAPI.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("transactiedatum")]
        [JsonPropertyOrder(2)]
        public string TransactionDate { get; init; }

        /// <summary>
        /// The earliest date/time when the request is sent by Expoints.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("sendTime")]
        [JsonPropertyOrder(3)]
        public string SendTime { get; init; }

        /// <summary>
        /// The customer data columns containing survey and service information.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("customerDataColumns")]
        [JsonPropertyOrder(5)]
        public CustomerDataColumns Columns { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KtoCustomerObject"/> struct.
        /// </summary>
        public KtoCustomerObject()
        {
            Emailadres = string.Empty;
            TransactionDate = string.Empty;
            SendTime = string.Empty;
            Columns = new CustomerDataColumns();
        }
    }

    /// <summary>
    /// Represents the customer data columns for surveys and services.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct CustomerDataColumns : IJsonSerializable
    {
        /// <summary>
        /// The name of the survey.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("vragenlijstnaam")]
        [JsonPropertyOrder(0)]
        public string SurveyName { get; init; }

        /// <summary>
        /// The name of the service.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("dienstnaam")]
        [JsonPropertyOrder(1)]
        public string ServiceName { get; init; }

        /// <summary>
        /// The type of measurement.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("typemeting")]
        [JsonPropertyOrder(2)]
        public string SurveyType { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerDataColumns"/> struct.
        /// </summary>
        public CustomerDataColumns()
        {
            SurveyName = string.Empty;
            ServiceName = string.Empty;
            SurveyType = string.Empty;
        }
    }
}