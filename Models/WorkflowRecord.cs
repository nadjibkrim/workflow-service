namespace WorkflowService.Models
{
    /// <summary>
    /// Represents a record whose state is managed by the workflow microservice.
    /// </summary>
    public class WorkflowRecord
    {
        /// <summary>
        /// Unique identifier of the record.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Optional name or display label for the record.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Current state of the record within the workflow.
        /// </summary>
        public string State { get; set; } = null!;

        /// <summary>
        /// Timestamp when the record was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp of the last state change. May be null if the record has not been updated since creation.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
