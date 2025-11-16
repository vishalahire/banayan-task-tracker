using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskTracker.Api.DTOs;
using TaskTracker.Application.Commands;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Queries;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    /// <summary>
    /// Search and filter tasks with pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<TaskSummaryResponse>>> SearchTasks(
        [FromQuery] TaskSearchRequest request,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        
        // Convert API request to application query
        var query = new TaskSearchQuery
        {
            SearchText = request.SearchText,
            Status = !string.IsNullOrEmpty(request.Status) && Enum.TryParse<TaskState>(request.Status, out var status) ? status : null,
            Priority = !string.IsNullOrEmpty(request.Priority) && Enum.TryParse<TaskPriority>(request.Priority, out var priority) ? priority : null,
            DueDateFrom = request.DueDateFrom,
            DueDateTo = request.DueDateTo,
            Tags = request.Tags,
            SortBy = request.SortBy ?? "CreatedAt",
            SortDescending = request.SortDescending,
            PageNumber = Math.Max(1, request.PageNumber),
            PageSize = Math.Max(1, Math.Min(100, request.PageSize))
        };

        var result = await _taskService.SearchTasksAsync(query, currentUserId, ct);

        // Convert to API response
        var response = new PagedResponse<TaskSummaryResponse>
        {
            Items = result.Items.Select(MapToSummaryResponse),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages,
            HasNextPage = result.HasNextPage,
            HasPreviousPage = result.HasPreviousPage
        };

        return Ok(response);
    }

    /// <summary>
    /// Get task details by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDetailsResponse>> GetTask(Guid id, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        
        var task = await _taskService.GetTaskByIdAsync(id, currentUserId, ct);
        
        if (task == null)
        {
            return NotFound();
        }

        var response = MapToDetailsResponse(task);
        return Ok(response);
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TaskDetailsResponse>> CreateTask([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();

        // Validate priority
        if (!Enum.TryParse<TaskPriority>(request.Priority, out var priority))
        {
            return BadRequest($"Invalid priority: {request.Priority}");
        }

        var command = new CreateTaskCommand
        {
            Title = request.Title,
            Description = request.Description,
            Priority = priority,
            DueDate = request.DueDate,
            Tags = request.Tags ?? new List<string>(),
            OwnerUserId = currentUserId
        };

        var taskId = await _taskService.CreateTaskAsync(command, ct);
        
        // Get the created task to return full details
        var createdTask = await _taskService.GetTaskByIdAsync(taskId, currentUserId, ct);
        var response = MapToDetailsResponse(createdTask!);

        return CreatedAtAction(nameof(GetTask), new { id = taskId }, response);
    }

    /// <summary>
    /// Update an existing task
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();

        // Validate priority
        if (!Enum.TryParse<TaskPriority>(request.Priority, out var priority))
        {
            return BadRequest($"Invalid priority: {request.Priority}");
        }

        var command = new UpdateTaskCommand
        {
            Id = id,
            Title = request.Title,
            Description = request.Description,
            Priority = priority,
            DueDate = request.DueDate,
            Tags = request.Tags ?? new List<string>(),
            CurrentUserId = currentUserId
        };

        try
        {
            await _taskService.UpdateTaskAsync(command, ct);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Update task status
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult> UpdateTaskStatus(Guid id, [FromBody] UpdateTaskStatusRequest request, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();

        // Validate status
        if (!Enum.TryParse<TaskState>(request.Status, out var status))
        {
            return BadRequest($"Invalid status: {request.Status}");
        }

        var command = new UpdateTaskStatusCommand
        {
            Id = id,
            Status = status,
            CurrentUserId = currentUserId
        };

        try
        {
            await _taskService.UpdateTaskStatusAsync(command, ct);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteTask(Guid id, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();

        try
        {
            await _taskService.DeleteTaskAsync(id, currentUserId, ct);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }

        return userId;
    }

    private static TaskDetailsResponse MapToDetailsResponse(Application.DTOs.TaskDetailsDto task)
    {
        return new TaskDetailsResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            Priority = task.Priority.ToString(),
            DueDate = task.DueDate,
            Tags = task.Tags,
            OwnerUserId = task.OwnerUserId,
            OwnerDisplayName = task.OwnerDisplayName,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            CompletedAt = task.CompletedAt,
            Attachments = task.Attachments.Select(MapToAttachmentResponse).ToList()
        };
    }

    private static TaskSummaryResponse MapToSummaryResponse(Application.DTOs.TaskSummaryDto task)
    {
        return new TaskSummaryResponse
        {
            Id = task.Id,
            Title = task.Title,
            Status = task.Status.ToString(),
            Priority = task.Priority.ToString(),
            DueDate = task.DueDate,
            Tags = task.Tags,
            OwnerUserId = task.OwnerUserId,
            OwnerDisplayName = task.OwnerDisplayName,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            AttachmentCount = task.AttachmentCount
        };
    }

    private static AttachmentResponse MapToAttachmentResponse(Application.DTOs.AttachmentDto attachment)
    {
        return new AttachmentResponse
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            FileSizeBytes = attachment.FileSizeBytes,
            UploadedByUserId = attachment.UploadedByUserId,
            UploadedByDisplayName = attachment.UploadedByDisplayName,
            CreatedAt = attachment.CreatedAt
        };
    }
}