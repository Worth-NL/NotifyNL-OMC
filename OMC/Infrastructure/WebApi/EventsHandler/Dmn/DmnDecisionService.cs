using net.adamec.lib.common.dmn.engine.engine.definition;
using net.adamec.lib.common.dmn.engine.engine.execution.context;
using net.adamec.lib.common.dmn.engine.engine.execution.result;
using net.adamec.lib.common.dmn.engine.parser;
using net.adamec.lib.common.dmn.engine.parser.dto;

namespace EventsHandler.Dmn
{
    /// <summary>
    /// 
    /// </summary>
    public class DmnDecisionService : IDmnDecisionService
    {
        private readonly DmnExecutionContext _ctx;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dmnPath"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public DmnDecisionService(string dmnPath)
        {
            string dmnPath1 = dmnPath ?? throw new ArgumentNullException(nameof(dmnPath));

            DmnModel dmnModel = DmnParser.Parse(dmnPath1) ?? throw new Exception("Failed to parse DMN model.");
            DmnDefinition? definition = DmnDefinitionFactory.CreateDmnDefinition(dmnModel);
            _ctx = DmnExecutionContextFactory.CreateExecutionContext(definition);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="decisionName"></param>
        /// <param name="inputParameters"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string? EvaluateDecision(string decisionName, Dictionary<string, object> inputParameters)
        {
            if (!_ctx.Definition.Decisions.ContainsKey(decisionName))
            {
                throw new Exception($"Decision '{decisionName}' not found in DMN definition.");
            }

            try
            {
                _ctx.WithInputParameters(inputParameters);
                DmnDecisionResult? result = _ctx.ExecuteDecision(decisionName);
                return result.Results.First().ToString();
            }
            catch(Exception ex)
            {
                Exception x = ex;
                throw;
            }
        }
    }
}
