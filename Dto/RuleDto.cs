namespace WorkflowService.Dto
{
    /// <summary>
    /// DTO for creating or updating a transition rule.
    /// </summary>
    public record CreateRuleDto(
        string FromState,
        string ToState,
        string ConditionExpression,
        int Priority = 0,
        string Description = ""
    );

    /// <summary>
    /// DTO for rule information returned by the API.
    /// </summary>
    public record RuleInfoDto(
        string FromState,
        string ToState,
        string Description,
        int Priority,
        bool IsActive
    );

    /// <summary>
    /// DTO for rule management with state machine context.
    /// </summary>
    public record RuleManagementDto(
        string StateMachineName,
        CreateRuleDto Rule
    );

    /// <summary>
    /// DTO for rule information with state machine context.
    /// </summary>
    public record RuleInfoWithContextDto(
        string StateMachineName,
        RuleInfoDto Rule
    );
} 