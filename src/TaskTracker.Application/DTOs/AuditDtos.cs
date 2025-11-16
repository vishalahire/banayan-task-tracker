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