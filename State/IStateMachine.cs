using WorkflowService.Models;

namespace WorkflowService.State
{
    /// <summary>
    /// Interface for the state machine that manages workflow transitions.
    /// </summary>
    public interface IStateMachine
    {
        /// <summary>
        /// Gets the initial state for new records.
        /// </summary>
        string InitialState { get; }

        /// <summary>
        /// Determines whether a transition from the current state to the requested state is permitted.
        /// </summary>
        bool CanTransition(string currentState, string nextState);

        /// <summary>
        /// Determines the next state for the given record based on defined conditional transitions.
        /// </summary>
        string? GetNextState(WorkflowRecord record);

        /// <summary>
        /// Gets all available transitions from the current state.
        /// </summary>
        IEnumerable<string> GetAvailableTransitions(string currentState);

        /// <summary>
        /// Gets all states in the workflow.
        /// </summary>
        IEnumerable<string> GetAllStates();
    }
} 