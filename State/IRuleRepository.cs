namespace WorkflowService.State
{
    /// <summary>
    /// Repository for managing workflow transition rules.
    /// </summary>
    public interface IRuleRepository
    {
        /// <summary>
        /// Gets all active rules for a given state.
        /// </summary>
        IEnumerable<TransitionRule> GetRulesForState(string state);

        /// <summary>
        /// Adds a new rule to the repository.
        /// </summary>
        void AddRule(TransitionRule rule);

        /// <summary>
        /// Removes a rule from the repository.
        /// </summary>
        void RemoveRule(string fromState, string toState);

        /// <summary>
        /// Updates an existing rule.
        /// </summary>
        void UpdateRule(TransitionRule rule);

        /// <summary>
        /// Gets all rules in the repository.
        /// </summary>
        IEnumerable<TransitionRule> GetAllRules();
    }
} 