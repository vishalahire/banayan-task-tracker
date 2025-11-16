using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.Entities;

public class AuditEvent
{
    public Guid Id { get; set; }
    public AuditAction Action { get; set; }
    public Guid UserId { get; set; }
    public Guid? EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public User? User { get; set; }

    public AuditEvent()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public AuditEvent(AuditAction action, Guid userId, Guid? entityId, string entityType, string details) : this()
    {
        Action = action;
        UserId = userId;
        EntityId = entityId;
        EntityType = entityType;
        Details = details;
    }

    public static AuditEvent TaskCreated(Guid userId, Guid taskId, string taskTitle)
    {
        return new AuditEvent(AuditAction.TaskCreated, userId, taskId, nameof(Task), $"Created task: {taskTitle}");
    }

    public static AuditEvent TaskUpdated(Guid userId, Guid taskId, string taskTitle)
    {
        return new AuditEvent(AuditAction.TaskUpdated, userId, taskId, nameof(Task), $"Updated task: {taskTitle}");
    }

    public static AuditEvent TaskDeleted(Guid userId, Guid taskId, string taskTitle)
    {
        return new AuditEvent(AuditAction.TaskDeleted, userId, taskId, nameof(Task), $"Deleted task: {taskTitle}");
    }

    public static AuditEvent TaskCompleted(Guid userId, Guid taskId, string taskTitle)
    {
        return new AuditEvent(AuditAction.TaskCompleted, userId, taskId, nameof(Task), $"Completed task: {taskTitle}");
    }

    public static AuditEvent AttachmentAdded(Guid userId, Guid attachmentId, string fileName, Guid taskId)
    {
        return new AuditEvent(AuditAction.AttachmentAdded, userId, attachmentId, nameof(Attachment), $"Added attachment '{fileName}' to task {taskId}");
    }

    public static AuditEvent AttachmentRemoved(Guid userId, Guid attachmentId, string fileName, Guid taskId)
    {
        return new AuditEvent(AuditAction.AttachmentRemoved, userId, attachmentId, nameof(Attachment), $"Removed attachment '{fileName}' from task {taskId}");
    }

    public static AuditEvent ReminderSent(Guid userId, Guid taskId, string taskTitle)
    {
        return new AuditEvent(AuditAction.ReminderSent, userId, taskId, nameof(Task), $"Sent reminder for task: {taskTitle}");
    }
}