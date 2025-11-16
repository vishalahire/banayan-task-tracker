namespace TaskTracker.Api.DTOs;

public class AuditEventResponse
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserDisplayName { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public class PagedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

public class PendingReminderResponse
{
    public Guid TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public DateTimeOffset DueDate { get; set; }
    public Guid OwnerUserId { get; set; }
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerDisplayName { get; set; } = string.Empty;
    public string ReminderType { get; set; } = string.Empty;
    public bool HasReminderBeenSent { get; set; }
    public double HoursUntilDue { get; set; }
}

public class ReminderProcessingResultResponse
{
    public int TotalPending { get; set; }
    public int ProcessedCount { get; set; }
    public int SuccessfulCount { get; set; }
    public int FailedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<Guid> ProcessedTaskIds { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}