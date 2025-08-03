using WebQueries.KTO.Models;

namespace WebQueries.KTO.Interfaces
{
    /// <summary>
    /// Interface to create a new instance of <see cref="KtoScenario"/>.
    /// </summary>
    public interface IKtoScenarioFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        KtoScenario Create();
    }
}