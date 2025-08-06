using WorkflowService.Dto;

namespace WorkflowService.State
{
    /// <summary>
    /// Represents a single state machine instance with its own rules and states.
    /// </summary>
    public class StateMachineInstance : IStateMachine, IStateMachineManager
    {
        private readonly IRuleRepository _ruleRepository;
        private readonly HashSet<string> _states = new(StringComparer.OrdinalIgnoreCase);
        private string _initialState = "New";
        private string _name = "Default Workflow";
        private string _id = "default-workflow";
        private DateTime _createdAt = DateTime.UtcNow;
        private DateTime _modifiedAt = DateTime.UtcNow;

        public StateMachineInstance(IRuleRepository ruleRepository)
        {
            _ruleRepository = ruleRepository;
            InitializeDefaultStates();
        }

        public string InitialState => _initialState;

        public bool CanTransition(string currentState, string nextState)
        {
            if (string.IsNullOrWhiteSpace(nextState) || string.IsNullOrWhiteSpace(currentState))
            {
                return false;
            }

            // Check if both states exist
            if (!StateExists(currentState) || !StateExists(nextState))
            {
                return false;
            }

            // Check if there's a rule for this transition
            var rules = _ruleRepository.GetRulesForState(currentState);
            return rules.Any(r => r.ToState.Equals(nextState, StringComparison.OrdinalIgnoreCase));
        }

        public string? GetNextState(WorkflowService.Models.WorkflowRecord record)
        {
            var rules = _ruleRepository.GetRulesForState(record.State);
            
            foreach (var rule in rules)
            {
                try
                {
                    if (rule.Condition.Invoke(record))
                    {
                        return rule.ToState;
                    }
                }
                catch (Exception ex)
                {
                    // In production, you'd want to log this error
                    Console.WriteLine($"Error evaluating rule {rule.Description}: {ex.Message}");
                    continue;
                }
            }

            return null;
        }

        public IEnumerable<string> GetAvailableTransitions(string currentState)
        {
            var rules = _ruleRepository.GetRulesForState(currentState);
            return rules.Select(r => r.ToState);
        }

        public IEnumerable<string> GetAllStates()
        {
            return _states.ToList();
        }

        // IStateMachineManager implementation
        public void CreateStateMachine(CreateStateMachineDto definition)
        {
            // Clear existing state machine
            _states.Clear();
            
            // Add new states
            foreach (var state in definition.States)
            {
                _states.Add(state.Name);
            }

            // Set initial state
            if (!_states.Contains(definition.InitialState))
            {
                throw new ArgumentException($"Initial state '{definition.InitialState}' is not defined in the states list.");
            }
            _initialState = definition.InitialState;
            _name = definition.Name;
            _id = definition.Id;
            _createdAt = DateTime.UtcNow;
            _modifiedAt = DateTime.UtcNow;

            // Clear existing rules and add new ones
            var existingRules = _ruleRepository.GetAllRules().ToList();
            foreach (var rule in existingRules)
            {
                _ruleRepository.RemoveRule(rule.FromState, rule.ToState);
            }

            // Add new transitions
            foreach (var transition in definition.Transitions)
            {
                if (!_states.Contains(transition.FromState))
                {
                    throw new ArgumentException($"From state '{transition.FromState}' is not defined.");
                }
                if (!_states.Contains(transition.ToState))
                {
                    throw new ArgumentException($"To state '{transition.ToState}' is not defined.");
                }

                var condition = CreateConditionFromExpression(transition.ConditionExpression);
                var rule = new TransitionRule
                {
                    FromState = transition.FromState,
                    ToState = transition.ToState,
                    Condition = condition,
                    Priority = transition.Priority,
                    Description = transition.Description,
                    IsActive = true
                };

                _ruleRepository.AddRule(rule);
            }
        }

        public StateMachineInfoDto GetStateMachineInfo()
        {
            var rules = _ruleRepository.GetAllRules()
                .Select(r => new RuleInfoDto(r.FromState, r.ToState, r.Description, r.Priority, r.IsActive))
                .ToList();

            return new StateMachineInfoDto(_id, _name, _initialState, _states, rules, _createdAt, _modifiedAt);
        }

        public void AddState(StateDefinitionDto state)
        {
            if (_states.Contains(state.Name))
            {
                throw new ArgumentException($"State '{state.Name}' already exists.");
            }

            _states.Add(state.Name);
            _modifiedAt = DateTime.UtcNow;
        }

        public void RemoveState(string stateName)
        {
            if (!_states.Contains(stateName))
            {
                throw new ArgumentException($"State '{stateName}' does not exist.");
            }

            if (stateName.Equals(_initialState, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Cannot remove the initial state.");
            }

            // Remove all rules involving this state
            var rulesToRemove = _ruleRepository.GetAllRules()
                .Where(r => r.FromState.Equals(stateName, StringComparison.OrdinalIgnoreCase) ||
                           r.ToState.Equals(stateName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var rule in rulesToRemove)
            {
                _ruleRepository.RemoveRule(rule.FromState, rule.ToState);
            }

            _states.Remove(stateName);
            _modifiedAt = DateTime.UtcNow;
        }

        public bool StateExists(string stateName)
        {
            return _states.Contains(stateName);
        }

        public string GetInitialState()
        {
            return _initialState;
        }

        private void InitializeDefaultStates()
        {
            _states.Add("New");
            _states.Add("InProgress");
            _states.Add("Completed");
            _states.Add("Cancelled");
            _states.Add("Archived");
        }

        private static Func<WorkflowService.Models.WorkflowRecord, bool> CreateConditionFromExpression(string expression)
        {
            return expression.ToLower() switch
            {
                "has_name" => record => !string.IsNullOrWhiteSpace(record.Name),
                "no_name" => record => string.IsNullOrWhiteSpace(record.Name),
                "older_than_10_minutes" => record => record.UpdatedAt.HasValue && 
                                                    (DateTime.UtcNow - record.UpdatedAt.Value).TotalMinutes > 10,
                "older_than_1_day" => record => (DateTime.UtcNow - record.CreatedAt).TotalDays >= 1,
                "always" => _ => true,
                "never" => _ => false,
                _ => _ => false
            };
        }
    }
} 