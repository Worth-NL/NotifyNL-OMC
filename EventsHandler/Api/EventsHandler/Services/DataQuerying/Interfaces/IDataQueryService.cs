// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.Interfaces;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataReceiving.Interfaces;

namespace EventsHandler.Services.DataQuerying.Interfaces
{
    /// <summary>
    /// The service obtaining data from external services.
    /// </summary>
    public interface IDataQueryService<in TModel>
        where TModel : struct, IJsonSerializable
    {
        /// <inheritdoc cref="IHttpNetworkService"/>
        internal IHttpNetworkService HttpNetwork { get; }

        /// <summary>
        /// Gets the query context of <see cref="IDataQueryService{TModel}"/> or sets it first if not yet existing.
        /// </summary>
        internal IQueryContext From(TModel model);
    }
}