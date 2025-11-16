using TaskTracker.Application.Commands;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Queries;
using TaskTracker.Application.Repositories;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditRepository _auditRepository;

    public TaskService(
        ITaskRepository taskRepository,
        IUserRepository userRepository,
        IAuditRepository auditRepository)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
        _auditRepository = auditRepository;
    }

    public async Task<Guid> CreateTaskAsync(CreateTaskCommand command, CancellationToken ct = default)
    {
        var task = new Domain.Entities.TaskItem(
            command.Title,
            command.Description,
            command.OwnerUserId,
            command.DueDate,
            command.Priority);

        if (command.Tags.Any())
        {
            task.Tags = command.Tags.ToList();
        }

        var taskId = await _taskRepository.AddAsync(task, ct);

        // Record audit event
        var auditEvent = AuditEvent.TaskCreated(command.OwnerUserId, taskId, command.Title);
        await _auditRepository.AddAsync(auditEvent, ct);

        return taskId;
    }

    public async Task UpdateTaskAsync(UpdateTaskCommand command, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(command.Id, ct);
        if (task == null)
        {
            throw new InvalidOperationException("Task not found");
        }

        // Check ownership
        if (!task.IsOwnedBy(command.CurrentUserId))
        {
            throw new UnauthorizedAccessException("You can only update your own tasks");
        }

        task.Update(command.Title, command.Description, command.Priority, command.DueDate, command.Tags);
        
        await _taskRepository.UpdateAsync(task, ct);

        // Record audit event
        var auditEvent = AuditEvent.TaskUpdated(command.CurrentUserId, command.Id, command.Title);
        await _auditRepository.AddAsync(auditEvent, ct);
    }

    public async Task UpdateTaskStatusAsync(UpdateTaskStatusCommand command, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(command.Id, ct);
        if (task == null)
        {
            throw new InvalidOperationException("Task not found");
        }

        // Check ownership
        if (!task.IsOwnedBy(command.CurrentUserId))
        {
            throw new UnauthorizedAccessException("You can only update your own tasks");
        }

        task.UpdateStatus(command.Status);
        
        await _taskRepository.UpdateAsync(task, ct);

        // Record audit event
        var auditAction = command.Status == TaskState.Completed ? AuditAction.TaskCompleted : AuditAction.TaskUpdated;
        var auditEvent = new AuditEvent(auditAction, command.CurrentUserId, command.Id, nameof(Domain.Entities.TaskItem), $"Status changed to {command.Status}");
        await _auditRepository.AddAsync(auditEvent, ct);
    }

    public async Task DeleteTaskAsync(Guid taskId, Guid currentUserId, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, ct);
        if (task == null)
        {
            throw new InvalidOperationException("Task not found");
        }

        // Check ownership
        if (!task.IsOwnedBy(currentUserId))
        {
            throw new UnauthorizedAccessException("You can only delete your own tasks");
        }

        await _taskRepository.DeleteAsync(taskId, ct);

        // Record audit event
        var auditEvent = AuditEvent.TaskDeleted(currentUserId, taskId, task.Title);
        await _auditRepository.AddAsync(auditEvent, ct);
    }

    public async Task<TaskDetailsDto?> GetTaskByIdAsync(Guid taskId, Guid currentUserId, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdWithOwnerAsync(taskId, ct);
        if (task == null)
        {
            return null;
        }

        // Users can view all tasks (as per requirements), but we still need the task to exist
        return new TaskDetailsDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            Tags = task.Tags,
            OwnerUserId = task.OwnerUserId,
            OwnerDisplayName = task.Owner?.DisplayName ?? "Unknown",
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            CompletedAt = task.CompletedAt,
            Attachments = task.Attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSizeBytes = a.FileSizeBytes,
                UploadedByUserId = a.UploadedByUserId,
                UploadedByDisplayName = a.UploadedBy?.DisplayName ?? "Unknown",
                CreatedAt = a.CreatedAt
            }).ToList()
        };
    }

    public async Task<PagedResult<TaskSummaryDto>> SearchTasksAsync(TaskSearchQuery query, Guid currentUserId, CancellationToken ct = default)
    {
        var skip = (query.PageNumber - 1) * query.PageSize;
        
        var tasks = await _taskRepository.SearchAsync(
            query.SearchText,
            query.Status,
            query.Priority,
            query.DueDateFrom,
            query.DueDateTo,
            query.Tags,
            query.SortBy ?? "CreatedAt",
            query.SortDescending,
            skip,
            query.PageSize,
            ct);

        var totalCount = await _taskRepository.CountAsync(
            query.SearchText,
            query.Status,
            query.Priority,
            query.DueDateFrom,
            query.DueDateTo,
            query.Tags,
            ct);

        var taskSummaries = tasks.Select(task => new TaskSummaryDto
        {
            Id = task.Id,
            Title = task.Title,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            Tags = task.Tags,
            OwnerUserId = task.OwnerUserId,
            OwnerDisplayName = task.Owner?.DisplayName ?? "Unknown",
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            AttachmentCount = task.Attachments.Count
        });

        return new PagedResult<TaskSummaryDto>
        {
            Items = taskSummaries,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<IEnumerable<ReminderTaskDto>> GetTasksDueSoonAsync(TimeSpan window, CancellationToken ct = default)
    {
        var tasks = await _taskRepository.GetTasksDueInWindowAsync(window, ct);
        
        return tasks.Select(task => new ReminderTaskDto
        {
            Id = task.Id,
            Title = task.Title,
            DueDate = task.DueDate!.Value,
            OwnerUserId = task.OwnerUserId,
            OwnerEmail = task.Owner?.Email ?? "unknown@example.com",
            OwnerDisplayName = task.Owner?.DisplayName ?? "Unknown"
        });
    }
}