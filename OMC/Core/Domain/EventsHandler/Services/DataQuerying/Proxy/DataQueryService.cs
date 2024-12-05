// © 2023, Worth Systems.

using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Proxy.Interfaces;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;

namespace EventsHandler.Services.DataQuerying.Proxy
{
    /// <inheritdoc cref="IDataQueryService{TModel}"/>
    internal sealed class DataQueryService : IDataQueryService<NotificationEvent>
    {
        private readonly IQueryContext _queryContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataQueryService"/> class.
        /// </summary>
        internal DataQueryService(IQueryContext queryContext)
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