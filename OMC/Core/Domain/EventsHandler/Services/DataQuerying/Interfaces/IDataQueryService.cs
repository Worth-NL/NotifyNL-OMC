// © 2023, Worth Systems.

using EventsHandler.Services.DataQuerying.Adapter.Interfaces;

namespace EventsHandler.Services.DataQuerying.Interfaces
{
    /// <summary>
    /// The service obtaining data from external services.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    public interface IDataQueryService<in TModel>
        where TModel : struct, IJsonSerializable
    {
        /// <summary>
        /// Gets the query context of <see cref="IDataQueryService{TModel}"/> or sets it first if not yet existing.
        /// </summary>
        internal IQueryContext From(TModel model);
    }
}