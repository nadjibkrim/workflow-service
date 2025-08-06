using WorkflowService.Models;

namespace WorkflowService.State
{
    /// <summary>
    /// Represents a transition rule from one state to another along with its predicate.
    /// </summary>
    public class TransitionRule
    {
        /// <summary>
        /// State from which this rule can be applied.
        /// </summary>
        public string FromState { get; set; } = null!;

        /// <summary>
        /// State to which this rule transitions when the condition is satisfied.
        /// </summary>
        public string ToState { get; set; } = null!;

        /// <summary>
        /// Condition that must be satisfied for the transition to occur.  
        /// It receives the record and returns true if the transition is allowed.
        /// </summary>
        public Func<WorkflowRecord, bool> Condition { get; set; } = _ => false;

        /// <summary>
        /// Priority of this rule. Higher priority rules are evaluated first.
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Human-readable description of what this rule does.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Whether this rule is currently active.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
