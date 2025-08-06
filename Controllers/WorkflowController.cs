using Microsoft.AspNetCore.Mvc;
using WorkflowService.Dto;
using WorkflowService.Models;
using WorkflowService.Services;

namespace WorkflowService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;

        public WorkflowController(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        /// <summary>
        /// Creates a new workflow record.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<WorkflowRecord>> CreateRecord(WorkflowRecordDto dto)
        {
            try
            {
                var record = await _workflowService.CreateRecordAsync(dto);
                return CreatedAtAction(nameof(GetRecord), new { id = record.Id }, record);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets a specific workflow record by ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<WorkflowRecord>> GetRecord(Guid id)
        {
            var record = await _workflowService.GetRecordAsync(id);
            return record is not null ? Ok(record) : NotFound();
        }

        /// <summary>
        /// Gets all workflow records.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkflowRecord>>> GetAllRecords()
        {
            var records = await _workflowService.GetAllRecordsAsync();
            return Ok(records);
        }

        /// <summary>
        /// Gets all workflow records for a specific state machine.
        /// </summary>
        [HttpGet("statemachine/{stateMachineId}")]
        public async Task<ActionResult<IEnumerable<WorkflowRecord>>> GetRecordsByStateMachine(string stateMachineId)
        {
            var records = await _workflowService.GetRecordsByStateMachineAsync(stateMachineId);
            return Ok(records);
        }

        /// <summary>
        /// Manually transitions a record to a specific state.
        /// </summary>
        [HttpPost("{id:guid}/transition")]
        public async Task<ActionResult<WorkflowRecord>> TransitionRecord(Guid id, StateTransitionDto dto)
        {
            try
            {
                var record = await _workflowService.TransitionRecordAsync(id, dto);
                return Ok(record);
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
        /// Automatically determines and applies the next state based on rules.
        /// </summary>
        [HttpPost("{id:guid}/next")]
        public async Task<ActionResult<WorkflowRecord>> NextState(Guid id)
        {
            try
            {
                var record = await _workflowService.NextStateAsync(id);
                return Ok(record);
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
    }
} 