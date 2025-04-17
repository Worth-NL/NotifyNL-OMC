namespace EventsHandler.Dmn
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDmnDecisionService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="decisionName"></param>
        /// <param name="inputParameters"></param>
        /// <returns></returns>
        string? EvaluateDecision(string decisionName, Dictionary<string, object> inputParameters);
    }
}
