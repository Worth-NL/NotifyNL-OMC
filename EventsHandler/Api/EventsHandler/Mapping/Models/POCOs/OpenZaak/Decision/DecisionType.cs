// © 2024, Worth Systems.

using EventsHandler.Mapping.Models.Interfaces;

namespace EventsHandler.Mapping.Models.POCOs.OpenZaak.Decision
{
    /// <summary>
    /// The type of the <see cref="Decision"/> retrieved from "OpenZaak" Web API service.
    /// </summary>
    /// <seealso cref="IJsonSerializable"/>
    public struct DecisionType : IJsonSerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecisionType"/> struct.
        /// </summary>
        public DecisionType()
        {
        }
    }
}