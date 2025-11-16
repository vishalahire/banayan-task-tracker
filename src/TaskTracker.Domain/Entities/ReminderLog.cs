namespace TaskTracker.Domain.Entities;

public class ReminderLog
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset ReminderSentAt { get; set; }
    public DateTimeOffset TaskDueDate { get; set; }
    public string ReminderType { get; set; } = string.Empty; // e.g., "24Hours", "1Hour", "Overdue"
    public bool DeliverySuccessful { get; set; }
    public string? DeliveryDetails { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public TaskItem? Task { get; set; }
    public User? User { get; set; }

    public ReminderLog()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
        ReminderSentAt = DateTimeOffset.UtcNow;
    }

    public ReminderLog(Guid taskId, Guid userId, DateTimeOffset taskDueDate, string reminderType) : this()
    {
        TaskId = taskId;
        UserId = userId;
        TaskDueDate = taskDueDate;
        ReminderType = reminderType;
        DeliverySuccessful = false;
    }

    public void MarkDelivered(bool successful, string? details = null)
    {
        DeliverySuccessful = successful;
        DeliveryDetails = details;
    }

    public static ReminderLog CreateForTask(TaskItem task, string reminderType)
    {
        if (!task.DueDate.HasValue)
            throw new InvalidOperationException("Cannot create reminder for task without due date");

        return new ReminderLog(task.Id, task.OwnerUserId, task.DueDate.Value, reminderType);
    }
}