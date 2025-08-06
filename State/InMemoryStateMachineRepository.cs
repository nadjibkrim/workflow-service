using WorkflowService.Dto;

namespace WorkflowService.State
{
    /// <summary>
    /// In-memory implementation of the state machine repository.
    /// In production, this could be replaced with a database-backed implementation.
    /// </summary>
    public class InMemoryStateMachineRepository : IStateMachineRepository
    {
        private readonly Dictionary<string, StateMachineInstance> _stateMachines = new(StringComparer.OrdinalIgnoreCase);
        private const string DefaultStateMachineId = "default-workflow";

        public InMemoryStateMachineRepository()
        {
            InitializeDefaultStateMachine();
        }

        public IEnumerable<StateMachineSummaryDto> GetAllStateMachines()
        {
            return _stateMachines.Values.Select(stateMachine =>
            {
                var info = stateMachine.GetStateMachineInfo();
                return new StateMachineSummaryDto(
                    Id: info.Id,
                    Name: info.Name,
                    InitialState: info.InitialState,
                    StateCount: info.States.Count(),
                    TransitionCount: info.Transitions.Count(),
                    CreatedAt: info.CreatedAt,
                    ModifiedAt: info.ModifiedAt
                );
            }).ToList();
        }

        public IStateMachineManager? GetStateMachine(string id)
        {
            return _stateMachines.TryGetValue(id, out var stateMachine) ? stateMachine : null;
        }

        public void CreateStateMachine(string id, CreateStateMachineDto definition)
        {
            if (_stateMachines.ContainsKey(id))
            {
                throw new ArgumentException($"State machine with ID '{id}' already exists.");
            }

            var stateMachine = new StateMachineInstance(new InMemoryRuleRepository());
            stateMachine.CreateStateMachine(definition);
            _stateMachines[id] = stateMachine;
        }

        public void UpdateStateMachine(string id, CreateStateMachineDto definition)
        {
            if (!_stateMachines.ContainsKey(id))
            {
                throw new ArgumentException($"State machine with ID '{id}' does not exist.");
            }

            var stateMachine = _stateMachines[id];
            stateMachine.CreateStateMachine(definition);
        }

        public void DeleteStateMachine(string id)
        {
            if (!_stateMachines.ContainsKey(id))
            {
                throw new ArgumentException($"State machine with ID '{id}' does not exist.");
            }

            if (id.Equals(DefaultStateMachineId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Cannot delete the default state machine.");
            }

            _stateMachines.Remove(id);
        }

        public bool StateMachineExists(string id)
        {
            return _stateMachines.ContainsKey(id);
        }

        public string GetDefaultStateMachineId()
        {
            return DefaultStateMachineId;
        }

        private void InitializeDefaultStateMachine()
        {
            var defaultDefinition = new CreateStateMachineDto(
                Id: DefaultStateMachineId,
                Name: "Default Workflow",
                InitialState: "New",
                States: new[]
                {
                    new StateDefinitionDto("New", "Initial state"),
                    new StateDefinitionDto("InProgress", "Work in progress"),
                    new StateDefinitionDto("Completed", "Work completed"),
                    new StateDefinitionDto("Cancelled", "Work cancelled"),
                    new StateDefinitionDto("Archived", "Work archived")
                },
                Transitions: new[]
                {
                    new TransitionDefinitionDto("New", "InProgress", "has_name", 10, "Move to InProgress when name is provided"),
                    new TransitionDefinitionDto("New", "Cancelled", "no_name", 5, "Cancel when name is not provided"),
                    new TransitionDefinitionDto("InProgress", "Completed", "older_than_10_minutes", 10, "Complete after 10 minutes of work"),
                    new TransitionDefinitionDto("Completed", "Archived", "older_than_1_day", 10, "Archive after 1 day")
                }
            );

            CreateStateMachine(DefaultStateMachineId, defaultDefinition);
        }
    }
} 