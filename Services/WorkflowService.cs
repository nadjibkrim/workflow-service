using Microsoft.EntityFrameworkCore;
using WorkflowService.Data;
using WorkflowService.Dto;
using WorkflowService.Models;
using WorkflowService.State;

namespace WorkflowService.Services
{
    /// <summary>
    /// Implementation of the workflow service.
    /// </summary>
    public class WorkflowService : IWorkflowService
    {
        private readonly ApplicationDbContext _db;
        private readonly IStateMachineRepository _stateMachineRepository;

        public WorkflowService(ApplicationDbContext db, IStateMachineRepository stateMachineRepository)
        {
            _db = db;
            _stateMachineRepository = stateMachineRepository;
        }

        public async Task<WorkflowRecord> CreateRecordAsync(WorkflowRecordDto dto)
        {
            // Validate that the state machine exists
            var stateMachineManager = _stateMachineRepository.GetStateMachine(dto.StateMachineId);
            if (stateMachineManager == null)
            {
                throw new ArgumentException($"State machine with ID '{dto.StateMachineId}' not found.");
            }

            // Cast to IStateMachine for workflow operations
            var stateMachine = (IStateMachine)stateMachineManager;

            var now = DateTime.UtcNow;
            var record = new WorkflowRecord
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                State = stateMachine.InitialState,
                StateMachineId = dto.StateMachineId,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _db.Records.AddAsync(record);
            await _db.SaveChangesAsync();

            return record;
        }

        public async Task<WorkflowRecord?> GetRecordAsync(Guid id)
        {
            return await _db.Records.FindAsync(id);
        }

        public async Task<IEnumerable<WorkflowRecord>> GetAllRecordsAsync()
        {
            return await _db.Records.ToListAsync();
        }

        public async Task<IEnumerable<WorkflowRecord>> GetRecordsByStateMachineAsync(string stateMachineId)
        {
            return await _db.Records
                .Where(r => r.StateMachineId == stateMachineId)
                .ToListAsync();
        }

        public async Task<WorkflowRecord> TransitionRecordAsync(Guid id, StateTransitionDto dto)
        {
            var record = await _db.Records.FindAsync(id);
            if (record is null)
            {
                throw new ArgumentException($"Record with ID {id} not found", nameof(id));
            }

            // Validate that the state machine exists and matches the record's state machine
            var stateMachineManager = _stateMachineRepository.GetStateMachine(dto.StateMachineId);
            if (stateMachineManager == null)
            {
                throw new ArgumentException($"State machine with ID '{dto.StateMachineId}' not found.");
            }

            if (record.StateMachineId != dto.StateMachineId)
            {
                throw new InvalidOperationException($"Record belongs to state machine '{record.StateMachineId}', not '{dto.StateMachineId}'.");
            }

            // Cast to IStateMachine for workflow operations
            var stateMachine = (IStateMachine)stateMachineManager;

            if (!stateMachine.CanTransition(record.State, dto.NextState))
            {
                throw new InvalidOperationException($"Invalid transition from '{record.State}' to '{dto.NextState}' in state machine '{dto.StateMachineId}'");
            }

            record.State = dto.NextState;
            record.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return record;
        }

        public async Task<WorkflowRecord> NextStateAsync(Guid id)
        {
            var record = await _db.Records.FindAsync(id);
            if (record is null)
            {
                throw new ArgumentException($"Record with ID {id} not found", nameof(id));
            }

            // Get the state machine for this record
            var stateMachineManager = _stateMachineRepository.GetStateMachine(record.StateMachineId);
            if (stateMachineManager == null)
            {
                throw new InvalidOperationException($"State machine '{record.StateMachineId}' not found for record {id}.");
            }

            // Cast to IStateMachine for workflow operations
            var stateMachine = (IStateMachine)stateMachineManager;

            var nextState = stateMachine.GetNextState(record);
            if (string.IsNullOrEmpty(nextState))
            {
                throw new InvalidOperationException($"No eligible transition from '{record.State}' based on defined conditions in state machine '{record.StateMachineId}'.");
            }

            record.State = nextState;
            record.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return record;
        }
    }
} 