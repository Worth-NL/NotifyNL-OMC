using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.KTO.Interfaces;
using WebQueries.KTO.Models;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;

namespace WebQueries.KTO
{
    internal sealed class KtoScenarioFactory : IKtoScenarioFactory
    {
        private readonly IDataQueryService<NotificationEvent> _dataQuery;
        private readonly IQueryContext _queryContext;

        public KtoScenarioFactory(
            IDataQueryService<NotificationEvent> dataQuery,
            IQueryContext queryContext)
        {
            _dataQuery = dataQuery;
            _queryContext = queryContext;
        }

        public KtoScenario Create()
        {
            return new KtoScenario(_dataQuery, _queryContext);
        }
    }
}