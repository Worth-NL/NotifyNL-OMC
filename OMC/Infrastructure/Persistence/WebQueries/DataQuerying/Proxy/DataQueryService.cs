// © 2023, Worth Systems.

using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Proxy.Interfaces;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;

namespace WebQueries.DataQuerying.Proxy
{
    /// <inheritdoc cref="IDataQueryService{TModel}"/>
    public sealed class DataQueryService : IDataQueryService<NotificationEvent>
    {
        private readonly IQueryContext _queryContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataQueryService"/> class.
        /// </summary>
        public DataQueryService(IQueryContext queryContext)  // Dependency Injection (DI)
        {
            this._queryContext = queryContext;
        }

        /// <inheritdoc cref="IDataQueryService{TModel}.From(TModel)"/>
        IQueryContext IDataQueryService<NotificationEvent>.From(NotificationEvent notification)
        {
            // Update only the current notification in cached builder
            this._queryContext.SetNotification(notification);

            return this._queryContext;
        }
    }
}