using System.Collections.Generic;
using System.Collections.ObjectModel;
using WorkflowService.Models;
using WorkflowService.Dto;

namespace WorkflowService.State
{
    /// <summary>
    /// Implementation of the state machine that uses a rule repository for flexible rule management.
    /// </summary>
    public class WorkflowStateMachine : IStateMachine
    {
        private readonly IRuleRepository _ruleRepository;
        private readonly IStateMachineManager _stateMachineManager;

        public WorkflowStateMachine(IRuleRepository ruleRepository, IStateMachineManager stateMachineManager)
        {
            _ruleRepository = ruleRepository;
            _stateMachineManager = stateMachineManager;
        }

        public string InitialState => _stateMachineManager.GetInitialState();

        public bool CanTransition(string currentState, string nextState)
        {
            if (string.IsNullOrWhiteSpace(nextState) || string.IsNullOrWhiteSpace(currentState))
            {
                return false;
            }

            // Check if both states exist
            if (!_stateMachineManager.StateExists(currentState) || !_stateMachineManager.StateExists(nextState))
            {
                return false;
            }

            // Check if there's a rule for this transition
            var rules = _ruleRepository.GetRulesForState(currentState);
            return rules.Any(r => r.ToState.Equals(nextState, StringComparison.OrdinalIgnoreCase));
        }

        public string? GetNextState(WorkflowRecord record)
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
            return _stateMachineManager.GetAllStates();
        }
    }
} 