using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskState Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public ICollection<string> Tags { get; set; } = new List<string>();
    public Guid OwnerUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    // Navigation properties
    public User? Owner { get; set; }
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public TaskItem()
    {
        Id = Guid.NewGuid();
        Status = TaskState.New;
        Priority = TaskPriority.Medium;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public TaskItem(string title, string description, Guid ownerUserId, DateTimeOffset? dueDate = null, TaskPriority priority = TaskPriority.Medium) : this()
    {
        Title = title;
        Description = description;
        OwnerUserId = ownerUserId;
        DueDate = dueDate;
        Priority = priority;
    }

    public void Update(string title, string description, TaskPriority priority, DateTimeOffset? dueDate, IEnumerable<string>? tags = null)
    {
        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate;
        
        if (tags != null)
        {
            Tags = tags.ToList();
        }
        
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkComplete()
    {
        Status = TaskState.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateStatus(TaskState status)
    {
        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
        
        if (status == TaskState.Completed && CompletedAt == null)
        {
            CompletedAt = DateTimeOffset.UtcNow;
        }
        else if (status != TaskState.Completed)
        {
            CompletedAt = null;
        }
    }

    public bool IsOwnedBy(Guid userId)
    {
        return OwnerUserId == userId;
    }

    public bool IsDueSoon(DateTimeOffset referenceTime, TimeSpan window)
    {
        return DueDate.HasValue && 
               DueDate.Value >= referenceTime && 
               DueDate.Value <= referenceTime.Add(window) &&
               Status != TaskState.Completed &&
               Status != TaskState.Archived;
    }
}