// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using System.Text.Json.Serialization;
using EventsHandler.Properties;
using Microsoft.IdentityModel.Tokens;

namespace EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v2
{
    /// <summary>
    /// The results about the parties (e.g., citizen, organization) retrieved from "OpenKlant" Web service.
    /// </summary>
    /// <remarks>
    ///   Version: "OpenKlant" (2.0) Web service.
    /// </remarks>
    /// <seealso cref="IJsonSerializable"/>
    public struct PartyResults : IJsonSerializable
    {
        /// <summary>
        /// The number of received results.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("count")]
        [JsonPropertyOrder(0)]
        public int Count { get; internal set; }

        /// <inheritdoc cref="PartyResult"/>
        [JsonInclude]
        [JsonPropertyName("results")]
        [JsonPropertyOrder(1)]
        public List<PartyResult> Results { get; internal set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyResults"/> struct.
        /// </summary>
        public PartyResults()
        {
        }

        /// <summary>
        /// Gets the <see cref="PartyResult"/>.
        /// </summary>
        /// <returns>
        ///   The data of a single party.
        /// </returns>
        /// <exception cref="HttpRequestException"/>
        internal readonly PartyResult Party()
        {
            if (this.Results.IsNullOrEmpty())
            {
                throw new HttpRequestException(Resources.HttpRequest_ERROR_EmptyPartiesResults);
            }

            foreach (PartyResult result in this.Results)
            {
                foreach (DigitalAddressLong address in result.Expansion.DigitalAddresses)
                {

                }
            }

            throw new NotImplementedException();
        }
    }
}