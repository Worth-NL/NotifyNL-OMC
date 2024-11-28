﻿// © 2023, Worth Systems.

using EventsHandler.Mapping.Models.POCOs.NotificatieApi;
using EventsHandler.Services.DataQuerying.Adapter.Interfaces;
using EventsHandler.Services.DataQuerying.Interfaces;

namespace EventsHandler.Services.DataQuerying
{
    /// <inheritdoc cref="IDataQueryService{TModel}"/>
    internal sealed class DataQueryService : IDataQueryService<NotificationEvent>
    {
        private readonly IQueryContext _queryContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataQueryService"/> class.
        /// </summary>
        public DataQueryService(IQueryContext queryContext)
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