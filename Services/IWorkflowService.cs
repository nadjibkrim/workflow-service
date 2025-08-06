using WorkflowService.Dto;
using WorkflowService.Models;

namespace WorkflowService.Services
{
    /// <summary>
    /// Service layer for workflow operations.
    /// </summary>
    public interface IWorkflowService
    {
        Task<WorkflowRecord> CreateRecordAsync(WorkflowRecordDto dto);
        Task<WorkflowRecord?> GetRecordAsync(Guid id);
        Task<IEnumerable<WorkflowRecord>> GetAllRecordsAsync();
        Task<IEnumerable<WorkflowRecord>> GetRecordsByStateMachineAsync(string stateMachineId);
        Task<WorkflowRecord> TransitionRecordAsync(Guid id, StateTransitionDto dto);
        Task<WorkflowRecord> NextStateAsync(Guid id);
    }
} 