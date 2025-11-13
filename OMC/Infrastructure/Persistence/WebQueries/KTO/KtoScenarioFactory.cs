using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataQuerying.Proxy.Interfaces;
using WebQueries.KTO.Interfaces;
using WebQueries.KTO.Models;
using ZhvModels.Mapping.Models.POCOs.NotificatieApi;

namespace WebQueries.KTO
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class KtoScenarioFactory : IKtoScenarioFactory
    {
        private readonly IDataQueryService<NotificationEvent> _dataQuery;
        private readonly IQueryContext _queryContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataQuery"></param>
        /// <param name="queryContext"></param>
        public KtoScenarioFactory(
            IDataQueryService<NotificationEvent> dataQuery,
            IQueryContext queryContext)
        {
            _dataQuery = dataQuery;
            _queryContext = queryContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public KtoScenario Create()
        {
            return new KtoScenario(_dataQuery, _queryContext);
        }
    }
}