using WorkflowService.Models;

namespace WorkflowService.State
{
    public interface IRuleBuilder
    {
        IRuleBuilder From(string state);
        IRuleBuilder To(string state);
        IRuleBuilder When(Func<WorkflowRecord, bool> condition);
        IRuleBuilder WithPriority(int priority);
        IRuleBuilder WithDescription(string description);
        TransitionRule Build();
    }

    public class RuleBuilder : IRuleBuilder
    {
        private string _fromState = string.Empty;
        private string _toState = string.Empty;
        private Func<WorkflowRecord, bool> _condition = _ => false;
        private int _priority = 0;
        private string _description = string.Empty;

        public IRuleBuilder From(string state)
        {
            _fromState = state;
            return this;
        }

        public IRuleBuilder To(string state)
        {
            _toState = state;
            return this;
        }

        public IRuleBuilder When(Func<WorkflowRecord, bool> condition)
        {
            _condition = condition;
            return this;
        }

        public IRuleBuilder WithPriority(int priority)
        {
            _priority = priority;
            return this;
        }

        public IRuleBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public TransitionRule Build()
        {
            return new TransitionRule
            {
                FromState = _fromState,
                ToState = _toState,
                Condition = _condition,
                Priority = _priority,
                Description = _description
            };
        }
    }
} 