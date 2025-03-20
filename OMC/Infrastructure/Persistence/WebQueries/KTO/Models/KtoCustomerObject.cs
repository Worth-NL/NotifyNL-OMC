using System.Text.Json.Serialization;

namespace WebQueries.KTO.Models
{
    /// <summary>
    /// Represents the root object containing customer data, and the request to be sent.
    /// </summary>
    public struct KtoCustomerObject : IJsonSerializable
    {
        /// <summary>
        /// Indicates whether to approve automatically.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("ApproveAutomatically")]
        [JsonPropertyOrder(1)]
        public bool ApproveAutomatically { get; init; }

        /// <summary>
        /// Indicates whether this is a test.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("IsTest")]
        [JsonPropertyOrder(2)]
        public bool IsTest { get; init; }

        /// <summary>
        /// The list of customers.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("Customers")]
        [JsonPropertyOrder(4)]
        public required Customer[] Customers { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KtoCustomerObject"/> struct.
        /// </summary>
        public KtoCustomerObject()
        {
        }
    }

    /// <summary>
    /// Represents a customer object.
    /// </summary>
    public struct Customer : IJsonSerializable
    {
        /// <summary>
        /// The email address of the customer.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("email")]
        [JsonPropertyOrder(1)]
        public required string Email { get; init; }

        /// <summary>
        /// The date when the request is sent.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("sendDate")]
        [JsonPropertyOrder(2)]
        public required DateOnly SendDate { get; init; }

        /// <summary>
        /// The transaction date.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("transactionDate")]
        [JsonPropertyOrder(3)]
        public required DateOnly TransactionDate { get; init; }

        /// <summary>
        /// The list of customer data columns.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("data")]
        [JsonPropertyOrder(4)]
        public required CustomerData[] Data { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> struct.
        /// </summary>
        public Customer()
        {
        }
    }

    /// <summary>
    /// Represents a customer data column.
    /// </summary>
    public struct CustomerData : IJsonSerializable
    {
        /// <summary>
        /// The ID of the customer data column.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("customerDataColumnId")]
        [JsonPropertyOrder(1)]
        public int CustomerDataColumnId { get; init; }

        /// <summary>
        /// The name of the data column.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("name")]
        [JsonPropertyOrder(2)]
        public required string Name { get; init; }

        /// <summary>
        /// The value of the data column.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("value")]
        [JsonPropertyOrder(3)]
        public required string Value { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerData"/> struct.
        /// </summary>
        public CustomerData()
        {
        }
    }
}