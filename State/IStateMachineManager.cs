using WorkflowService.Dto;

namespace WorkflowService.State
{
    /// <summary>
    /// Manages complete state machine definitions.
    /// </summary>
    public interface IStateMachineManager
    {
        /// <summary>
        /// Creates a new state machine definition.
        /// </summary>
        void CreateStateMachine(CreateStateMachineDto definition);

        /// <summary>
        /// Gets information about the current state machine.
        /// </summary>
        StateMachineInfoDto GetStateMachineInfo();

        /// <summary>
        /// Adds a new state to the state machine.
        /// </summary>
        void AddState(StateDefinitionDto state);

        /// <summary>
        /// Removes a state from the state machine.
        /// </summary>
        void RemoveState(string stateName);

        /// <summary>
        /// Gets all states in the state machine.
        /// </summary>
        IEnumerable<string> GetAllStates();

        /// <summary>
        /// Validates that a state exists in the state machine.
        /// </summary>
        bool StateExists(string stateName);

        /// <summary>
        /// Gets the initial state of the state machine.
        /// </summary>
        string GetInitialState();
    }
} 