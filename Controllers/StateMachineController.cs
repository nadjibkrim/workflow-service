using Microsoft.AspNetCore.Mvc;
using WorkflowService.Dto;
using WorkflowService.State;

namespace WorkflowService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StateMachineController : ControllerBase
    {
        private readonly IStateMachineRepository _stateMachineRepository;

        public StateMachineController(IStateMachineRepository stateMachineRepository)
        {
            _stateMachineRepository = stateMachineRepository;
        }

        /// <summary>
        /// Gets all state machines with summary information.
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<StateMachineSummaryDto>> GetAllStateMachines()
        {
            var stateMachines = _stateMachineRepository.GetAllStateMachines();
            return Ok(stateMachines);
        }

        /// <summary>
        /// Gets information about a specific state machine.
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<StateMachineInfoDto> GetStateMachineInfo(string id)
        {
            var stateMachine = _stateMachineRepository.GetStateMachine(id);
            if (stateMachine == null)
            {
                return NotFound(new { message = $"State machine with ID '{id}' not found." });
            }

            var info = stateMachine.GetStateMachineInfo();
            return Ok(info);
        }

        /// <summary>
        /// Creates a new state machine definition.
        /// </summary>
        [HttpPost]
        public ActionResult CreateStateMachine(CreateStateMachineDto definition)
        {
            try
            {
                _stateMachineRepository.CreateStateMachine(definition.Id, definition);
                return CreatedAtAction(nameof(GetStateMachineInfo), new { id = definition.Id }, definition);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing state machine.
        /// </summary>
        [HttpPut("{id}")]
        public ActionResult UpdateStateMachine(string id, CreateStateMachineDto definition)
        {
            try
            {
                _stateMachineRepository.UpdateStateMachine(id, definition);
                return Ok(definition);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a state machine.
        /// </summary>
        [HttpDelete("{id}")]
        public ActionResult DeleteStateMachine(string id)
        {
            try
            {
                _stateMachineRepository.DeleteStateMachine(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets all states in a specific state machine.
        /// </summary>
        [HttpGet("{id}/states")]
        public ActionResult<IEnumerable<string>> GetAllStates(string id)
        {
            var stateMachine = _stateMachineRepository.GetStateMachine(id);
            if (stateMachine == null)
            {
                return NotFound(new { message = $"State machine with ID '{id}' not found." });
            }

            var states = stateMachine.GetAllStates();
            return Ok(states);
        }

        /// <summary>
        /// Adds a new state to a specific state machine.
        /// </summary>
        [HttpPost("{id}/states")]
        public ActionResult AddState(string id, StateDefinitionDto state)
        {
            var stateMachine = _stateMachineRepository.GetStateMachine(id);
            if (stateMachine == null)
            {
                return NotFound(new { message = $"State machine with ID '{id}' not found." });
            }

            try
            {
                stateMachine.AddState(state);
                return CreatedAtAction(nameof(GetAllStates), new { id }, state);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Removes a state from a specific state machine.
        /// </summary>
        [HttpDelete("{id}/states/{stateName}")]
        public ActionResult RemoveState(string id, string stateName)
        {
            var stateMachine = _stateMachineRepository.GetStateMachine(id);
            if (stateMachine == null)
            {
                return NotFound(new { message = $"State machine with ID '{id}' not found." });
            }

            try
            {
                stateMachine.RemoveState(stateName);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets the initial state of a specific state machine.
        /// </summary>
        [HttpGet("{id}/initial-state")]
        public ActionResult<string> GetInitialState(string id)
        {
            var stateMachine = _stateMachineRepository.GetStateMachine(id);
            if (stateMachine == null)
            {
                return NotFound(new { message = $"State machine with ID '{id}' not found." });
            }

            var initialState = stateMachine.GetInitialState();
            return Ok(initialState);
        }
    }
} 