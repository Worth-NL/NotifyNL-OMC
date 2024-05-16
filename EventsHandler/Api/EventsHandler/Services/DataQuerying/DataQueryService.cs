// © 2023, Worth Systems.

using EventsHandler.Behaviors.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataQuerying.Interfaces;
using EventsHandler.Services.DataQuerying.Strategy.Interfaces;
using EventsHandler.Services.DataReceiving.Interfaces;

namespace EventsHandler.Services.DataQuerying
{
    /// <inheritdoc cref="IDataQueryService{TModel}"/>
    internal sealed class DataQueryService : IDataQueryService<NotificationEvent>
    {
        private readonly IQueryContext _queryContext;

        /// <inheritdoc cref="IDataQueryService{TModel}.HttpSupplier"/>
        public IHttpSupplierService HttpSupplier { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataQueryService"/> class.
        /// </summary>
        public DataQueryService(IQueryContext queryContext, IHttpSupplierService httpSupplier)
        {
            this._queryContext = queryContext;

            this.HttpSupplier = httpSupplier;
        }

        /// <inheritdoc cref="IDataQueryService{TModel}.From(TModel)"/>
        IQueryContext IDataQueryService<NotificationEvent>.From(NotificationEvent notification)
        {
            // Update only the current notification in cached builder
            this._queryContext.Notification = notification;

            return this._queryContext;
        }
    }
}