using System.ComponentModel.DataAnnotations;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Api.DTOs;

public class TaskDetailsResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTimeOffset? DueDate { get; set; }
    public ICollection<string> Tags { get; set; } = new List<string>();
    public Guid OwnerUserId { get; set; }
    public string OwnerDisplayName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public ICollection<AttachmentResponse> Attachments { get; set; } = new List<AttachmentResponse>();
}

public class TaskSummaryResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTimeOffset? DueDate { get; set; }
    public ICollection<string> Tags { get; set; } = new List<string>();
    public Guid OwnerUserId { get; set; }
    public string OwnerDisplayName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int AttachmentCount { get; set; }
}

public class CreateTaskRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string Priority { get; set; } = string.Empty;
    
    public DateTimeOffset? DueDate { get; set; }
    
    public ICollection<string> Tags { get; set; } = new List<string>();
}

public class UpdateTaskRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string Priority { get; set; } = string.Empty;
    
    public DateTimeOffset? DueDate { get; set; }
    
    public ICollection<string> Tags { get; set; } = new List<string>();
}

public class UpdateTaskStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}

public class TaskSearchRequest
{
    public string? SearchText { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public DateTimeOffset? DueDateFrom { get; set; }
    public DateTimeOffset? DueDateTo { get; set; }
    public ICollection<string>? Tags { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}