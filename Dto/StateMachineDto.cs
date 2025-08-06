namespace WorkflowService.Dto
{
    /// <summary>
    /// DTO for creating a complete state machine definition.
    /// </summary>
    public record CreateStateMachineDto(
        string Id,
        string Name,
        string InitialState,
        IEnumerable<StateDefinitionDto> States,
        IEnumerable<TransitionDefinitionDto> Transitions
    );

    /// <summary>
    /// DTO for defining a state in the state machine.
    /// </summary>
    public record StateDefinitionDto(
        string Name,
        string Description,
        bool IsFinal = false,
        bool IsTerminal = false
    );

    /// <summary>
    /// DTO for defining a transition between states.
    /// </summary>
    public record TransitionDefinitionDto(
        string FromState,
        string ToState,
        string ConditionExpression,
        int Priority = 0,
        string Description = ""
    );

    /// <summary>
    /// DTO for state machine information.
    /// </summary>
    public record StateMachineInfoDto(
        string Id,
        string Name,
        string InitialState,
        IEnumerable<string> States,
        IEnumerable<RuleInfoDto> Transitions,
        DateTime CreatedAt,
        DateTime ModifiedAt
    );

    public record StateMachineSummaryDto(
        string Id,
        string Name,
        string InitialState,
        int StateCount,
        int TransitionCount,
        DateTime CreatedAt,
        DateTime ModifiedAt
    );
} 