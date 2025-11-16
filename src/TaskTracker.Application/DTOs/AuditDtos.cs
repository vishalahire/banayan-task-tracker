using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.DTOs;

public class AuditEventDto
{
    public Guid Id { get; set; }
    public AuditAction Action { get; set; }
    public Guid UserId { get; set; }
    public string UserDisplayName { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public class ReminderTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTimeOffset DueDate { get; set; }
    public Guid OwnerUserId { get; set; }
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerDisplayName { get; set; } = string.Empty;
}

public class PendingReminderDto
{
    public Guid TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public DateTimeOffset DueDate { get; set; }
    public Guid OwnerUserId { get; set; }
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerDisplayName { get; set; } = string.Empty;
    public string ReminderType { get; set; } = string.Empty;
    public bool HasReminderBeenSent { get; set; }
    public TimeSpan TimeUntilDue { get; set; }
}

public class ReminderProcessingResultDto
{
    public int TotalPending { get; set; }
    public int ProcessedCount { get; set; }
    public int SuccessfulCount { get; set; }
    public int FailedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<Guid> ProcessedTaskIds { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}