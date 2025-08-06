using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WorkflowService.State
{
    /// <summary>
    /// In-memory implementation of the rule repository.
    /// In production, this could be replaced with a database-backed implementation.
    /// </summary>
    public class InMemoryRuleRepository : IRuleRepository
    {
        private readonly List<TransitionRule> _rules = new();
        private readonly object _lock = new();

        public InMemoryRuleRepository()
        {
            // Initialize with default rules
            InitializeDefaultRules();
        }

        public IEnumerable<TransitionRule> GetRulesForState(string state)
        {
            lock (_lock)
            {
                return _rules
                    .Where(r => r.FromState.Equals(state, StringComparison.OrdinalIgnoreCase) && r.IsActive)
                    .OrderByDescending(r => r.Priority)
                    .ToList();
            }
        }

        public void AddRule(TransitionRule rule)
        {
            lock (_lock)
            {
                _rules.Add(rule);
            }
        }

        public void RemoveRule(string fromState, string toState)
        {
            lock (_lock)
            {
                var rule = _rules.FirstOrDefault(r => 
                    r.FromState.Equals(fromState, StringComparison.OrdinalIgnoreCase) &&
                    r.ToState.Equals(toState, StringComparison.OrdinalIgnoreCase));
                
                if (rule != null)
                {
                    _rules.Remove(rule);
                }
            }
        }

        public void UpdateRule(TransitionRule rule)
        {
            lock (_lock)
            {
                var existingRule = _rules.FirstOrDefault(r => 
                    r.FromState.Equals(rule.FromState, StringComparison.OrdinalIgnoreCase) &&
                    r.ToState.Equals(rule.ToState, StringComparison.OrdinalIgnoreCase));
                
                if (existingRule != null)
                {
                    var index = _rules.IndexOf(existingRule);
                    _rules[index] = rule;
                }
            }
        }

        public IEnumerable<TransitionRule> GetAllRules()
        {
            lock (_lock)
            {
                return _rules.ToList();
            }
        }

        private void InitializeDefaultRules()
        {
            var builder = new RuleBuilder();

            // New -> InProgress (when name is provided)
            AddRule(builder
                .From("New")
                .To("InProgress")
                .When(record => !string.IsNullOrWhiteSpace(record.Name))
                .WithPriority(10)
                .WithDescription("Move to InProgress when name is provided")
                .Build());

            // New -> Cancelled (when name is empty)
            AddRule(builder
                .From("New")
                .To("Cancelled")
                .When(record => string.IsNullOrWhiteSpace(record.Name))
                .WithPriority(5)
                .WithDescription("Cancel when name is not provided")
                .Build());

            // InProgress -> Completed (after 10 minutes)
            AddRule(builder
                .From("InProgress")
                .To("Completed")
                .When(record => record.UpdatedAt.HasValue && 
                               (DateTime.UtcNow - record.UpdatedAt.Value).TotalMinutes > 10)
                .WithPriority(10)
                .WithDescription("Complete after 10 minutes of work")
                .Build());

            // Completed -> Archived (after 1 day)
            AddRule(builder
                .From("Completed")
                .To("Archived")
                .When(record => (DateTime.UtcNow - record.CreatedAt).TotalDays >= 1)
                .WithPriority(10)
                .WithDescription("Archive after 1 day")
                .Build());
        }
    }
} 