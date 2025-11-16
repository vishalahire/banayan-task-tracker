using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.Queries;

public class TaskSearchQuery
{
    public string? SearchText { get; set; }
    public TaskState? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public DateTimeOffset? DueDateFrom { get; set; }
    public DateTimeOffset? DueDateTo { get; set; }
    public ICollection<string>? Tags { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}