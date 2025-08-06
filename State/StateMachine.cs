using System.Collections.ObjectModel;
using WorkflowService.Models;

namespace WorkflowService.State
{
    /// <summary>
    /// Defines a state machine that supports both static allowed transitions and
    /// conditional automatic transitions via a Next() operation.
    /// </summary>
    public static class StateMachine
    {
        /// <summary>
        /// Initial state assigned to new records.
        /// </summary>
        public const string InitialState = "New";

        /// <summary>
        /// Defines unconditional allowed transitions for validation when the caller
        /// explicitly requests a specific target state.  This is left for
        /// compatibility but not used by the automatic Next() operation.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> AllowedTransitions;

        /// <summary>
        /// Defines conditional transitions for automatic Next() operations.
        /// Each entry lists rules (ordered) from the current state to candidate
        /// next states along with conditions that must be satisfied for the rule
        /// to fire.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, IReadOnlyCollection<TransitionRule>> ConditionalTransitions;

        static StateMachine()
        {
            // Unconditional transitions for manual requests
            var transitions = new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["New"] = new ReadOnlyCollection<string>(new[] { "InProgress", "Cancelled" }),
                ["InProgress"] = new ReadOnlyCollection<string>(new[] { "Completed", "Cancelled" }),
                ["Completed"] = new ReadOnlyCollection<string>(new[] { "Archived" }),
                ["Cancelled"] = Array.Empty<string>(),
                ["Archived"] = Array.Empty<string>(),
            };
            AllowedTransitions = new ReadOnlyDictionary<string, IReadOnlyCollection<string>>(transitions);

            // Conditional transitions for automatic Next() operations
            // Example conditions:  
            //  - A record in 'New' state moves to 'InProgress' when it has a non-empty name.  
            //  - A record in 'InProgress' moves to 'Completed' when a hypothetical boolean property 'IsDone' is true.  
            //  - Completed records automatically move to 'Archived' after creation time is older than one day.  
            //  You can modify or extend these conditions based on your domain.
            var conditional = new Dictionary<string, IReadOnlyCollection<TransitionRule>>(StringComparer.OrdinalIgnoreCase)
            {
                ["New"] = new ReadOnlyCollection<TransitionRule>(new[]
                {
                    new TransitionRule
                    {
                        FromState = "New",
                        ToState = "InProgress",
                        Condition = record => !string.IsNullOrWhiteSpace(record.Name)
                    },
                    new TransitionRule
                    {
                        FromState = "New",
                        ToState = "Cancelled",
                        Condition = record => string.IsNullOrWhiteSpace(record.Name)
                    }
                }),
                ["InProgress"] = new ReadOnlyCollection<TransitionRule>(new[]
                {
                    new TransitionRule
                    {
                        FromState = "InProgress",
                        ToState = "Completed",
                        Condition = record =>
                        {
                            // You can extend the WorkflowRecord with custom flags.  
                            // For demonstration we assume there is a dynamic property bag or similar.  
                            // Here we simply transition after 10 minutes of work.  
                            return record.UpdatedAt.HasValue && (DateTime.UtcNow - record.UpdatedAt.Value).TotalMinutes > 10;
                        }
                    },
                    new TransitionRule
                    {
                        FromState = "InProgress",
                        ToState = "Cancelled",
                        Condition = record => false // you can define more conditions
                    }
                }),
                ["Completed"] = new ReadOnlyCollection<TransitionRule>(new[]
                {
                    new TransitionRule
                    {
                        FromState = "Completed",
                        ToState = "Archived",
                        Condition = record => (DateTime.UtcNow - record.CreatedAt).TotalDays >= 1
                    }
                }),
                ["Cancelled"] = Array.Empty<TransitionRule>(),
                ["Archived"] = Array.Empty<TransitionRule>(),
            };
            ConditionalTransitions = new ReadOnlyDictionary<string, IReadOnlyCollection<TransitionRule>>(conditional);
        }

        /// <summary>
        /// Determines whether a transition from the current state to the requested state is permitted for manual transitions.
        /// </summary>
        public static bool CanTransition(string currentState, string nextState)
        {
            if (string.IsNullOrWhiteSpace(nextState) || string.IsNullOrWhiteSpace(currentState))
            {
                return false;
            }
            if (!AllowedTransitions.TryGetValue(currentState, out var targets))
            {
                return false;
            }
            return targets.Contains(nextState, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines the next state for the given record based on defined conditional transitions.  
        /// Returns null if no conditions are satisfied or if no transitions are defined.
        /// </summary>
        public static string? GetNextState(WorkflowRecord record)
        {
            if (!ConditionalTransitions.TryGetValue(record.State, out var rules))
            {
                return null;
            }
            foreach (var rule in rules)
            {
                if (rule.Condition.Invoke(record))
                {
                    return rule.ToState;
                }
            }
            return null;
        }
    }
}
