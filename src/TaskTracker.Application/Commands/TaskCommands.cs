using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.Commands;

public class CreateTaskCommand
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public ICollection<string> Tags { get; set; } = new List<string>();
    public Guid OwnerUserId { get; set; }
}

public class UpdateTaskCommand
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public ICollection<string> Tags { get; set; } = new List<string>();
    public Guid CurrentUserId { get; set; }
}

public class UpdateTaskStatusCommand
{
    public Guid Id { get; set; }
    public TaskState Status { get; set; }
    public Guid CurrentUserId { get; set; }
}