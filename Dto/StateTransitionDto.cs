namespace WorkflowService.Dto
{
    /// <summary>
    /// Data transfer object used when requesting a state change.
    /// </summary>
    public record StateTransitionDto(string NextState, string StateMachineId);
}
