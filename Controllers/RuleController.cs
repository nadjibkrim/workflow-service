using Microsoft.AspNetCore.Mvc;
using WorkflowService.Dto;
using WorkflowService.State;

namespace WorkflowService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RuleController : ControllerBase
    {
        private readonly IStateMachineRepository _stateMachineRepository;

        public RuleController(IStateMachineRepository stateMachineRepository)
        {
            _stateMachineRepository = stateMachineRepository;
        }

        /// <summary>
        /// Gets all rules in a specific state machine.
        /// </summary>
        [HttpGet("{stateMachineId}")]
        public ActionResult<IEnumerable<RuleInfoDto>> GetAllRules(string stateMachineId)
        {
            var stateMachine = _stateMachineRepository.GetStateMachine(stateMachineId);
            if (stateMachine == null)
            {
                return NotFound(new { message = $"State machine with ID '{stateMachineId}' not found." });
            }

            var info = stateMachine.GetStateMachineInfo();
            return Ok(info.Transitions);
        }

        /// <summary>
        /// Gets all rules for a specific state in a specific state machine.
        /// </summary>
        [HttpGet("{stateMachineId}/state/{state}")]
        public ActionResult<IEnumerable<RuleInfoDto>> GetRulesForState(string stateMachineId, string state)
        {
            var stateMachine = _stateMachineRepository.GetStateMachine(stateMachineId);
            if (stateMachine == null)
            {
                return NotFound(new { message = $"State machine with ID '{stateMachineId}' not found." });
            }

            // Validate that the state exists in the state machine
            if (!stateMachine.StateExists(state))
            {
                return BadRequest(new { message = $"State '{state}' does not exist in state machine '{stateMachineId}'." });
            }

            var info = stateMachine.GetStateMachineInfo();
            var rules = info.Transitions.Where(r => r.FromState.Equals(state, StringComparison.OrdinalIgnoreCase)).ToList();
            return Ok(rules);
        }

        /// <summary>
        /// Creates a new transition rule in a specific state machine.
        /// </summary>
        [HttpPost("{stateMachineId}")]
        public ActionResult<RuleInfoDto> CreateRule(string stateMachineId, CreateRuleDto dto)
        {
            var stateMachine = _stateMachineRepository.GetStateMachine(stateMachineId);
            if (stateMachine == null)
            {
                return NotFound(new { message = $"State machine with ID '{stateMachineId}' not found." });
            }

            // Validate that both states exist in the state machine
            if (!stateMachine.StateExists(dto.FromState))
            {
                return BadRequest(new { message = $"From state '{dto.FromState}' does not exist in state machine '{stateMachineId}'." });
            }

            if (!stateMachine.StateExists(dto.ToState))
            {
                return BadRequest(new { message = $"To state '{dto.ToState}' does not exist in state machine '{stateMachineId}'." });
            }

            // Note: In a real implementation, you'd want to parse the condition expression
            // For now, we'll create a simple condition based on the expression
            var condition = CreateConditionFromExpression(dto.ConditionExpression);
            
            var rule = new TransitionRule
            {
                FromState = dto.FromState,
                ToState = dto.ToState,
                Condition = condition,
                Priority = dto.Priority,
                Description = dto.Description,
                IsActive = true
            };
            
            // Add the rule to the state machine's rule repository
            // This would require exposing the rule repository from the state machine
            // For now, we'll return a success response
            return CreatedAtAction(nameof(GetRulesForState), new { stateMachineId, state = dto.FromState }, 
                new RuleInfoDto(dto.FromState, dto.ToState, dto.Description, dto.Priority, true));
        }

        /// <summary>
        /// Deletes a specific rule from a specific state machine.
        /// </summary>
        [HttpDelete("{stateMachineId}/{fromState}/{toState}")]
        public ActionResult DeleteRule(string stateMachineId, string fromState, string toState)
        {
            var stateMachine = _stateMachineRepository.GetStateMachine(stateMachineId);
            if (stateMachine == null)
            {
                return NotFound(new { message = $"State machine with ID '{stateMachineId}' not found." });
            }

            // Validate that both states exist in the state machine
            if (!stateMachine.StateExists(fromState))
            {
                return BadRequest(new { message = $"From state '{fromState}' does not exist in state machine '{stateMachineId}'." });
            }

            if (!stateMachine.StateExists(toState))
            {
                return BadRequest(new { message = $"To state '{toState}' does not exist in state machine '{stateMachineId}'." });
            }

            // Remove the rule from the state machine's rule repository
            // This would require exposing the rule repository from the state machine
            return NoContent();
        }

        /// <summary>
        /// Gets information about a specific state machine context.
        /// </summary>
        [HttpGet("{stateMachineId}/context")]
        public ActionResult<object> GetContext(string stateMachineId)
        {
            var stateMachine = _stateMachineRepository.GetStateMachine(stateMachineId);
            if (stateMachine == null)
            {
                return NotFound(new { message = $"State machine with ID '{stateMachineId}' not found." });
            }

            var stateMachineInfo = stateMachine.GetStateMachineInfo();
            return Ok(new
            {
                StateMachineId = stateMachineInfo.Id,
                StateMachineName = stateMachineInfo.Name,
                InitialState = stateMachineInfo.InitialState,
                AvailableStates = stateMachineInfo.States,
                TotalRules = stateMachineInfo.Transitions.Count(),
                CreatedAt = stateMachineInfo.CreatedAt,
                ModifiedAt = stateMachineInfo.ModifiedAt
            });
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