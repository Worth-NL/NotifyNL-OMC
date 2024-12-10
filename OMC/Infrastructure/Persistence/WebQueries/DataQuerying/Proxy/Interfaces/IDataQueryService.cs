// © 2023, Worth Systems.

using WebQueries.DataQuerying.Adapter.Interfaces;
using ZhvModels.Mapping.Models.Interfaces;

namespace WebQueries.DataQuerying.Proxy.Interfaces
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
        public IQueryContext From(TModel model);
    }
}