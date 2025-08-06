using WorkflowService.Dto;

namespace WorkflowService.State
{
    /// <summary>
    /// Repository for managing multiple state machines.
    /// </summary>
    public interface IStateMachineRepository
    {
        /// <summary>
        /// Gets all state machines with summary information.
        /// </summary>
        IEnumerable<StateMachineSummaryDto> GetAllStateMachines();

        /// <summary>
        /// Gets a specific state machine by ID.
        /// </summary>
        IStateMachineManager? GetStateMachine(string id);

        /// <summary>
        /// Creates a new state machine.
        /// </summary>
        void CreateStateMachine(string id, CreateStateMachineDto definition);

        /// <summary>
        /// Updates an existing state machine.
        /// </summary>
        void UpdateStateMachine(string id, CreateStateMachineDto definition);

        /// <summary>
        /// Deletes a state machine.
        /// </summary>
        void DeleteStateMachine(string id);

        /// <summary>
        /// Checks if a state machine exists.
        /// </summary>
        bool StateMachineExists(string id);

        /// <summary>
        /// Gets the default state machine ID.
        /// </summary>
        string GetDefaultStateMachineId();
    }
} 