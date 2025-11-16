using TaskTracker.Application.Commands;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Queries;

namespace TaskTracker.Application.Interfaces;

public interface ITaskService
{
    Task<Guid> CreateTaskAsync(CreateTaskCommand command, CancellationToken ct = default);
    Task UpdateTaskAsync(UpdateTaskCommand command, CancellationToken ct = default);
    Task UpdateTaskStatusAsync(UpdateTaskStatusCommand command, CancellationToken ct = default);
    Task DeleteTaskAsync(Guid taskId, Guid currentUserId, CancellationToken ct = default);
    Task<TaskDetailsDto?> GetTaskByIdAsync(Guid taskId, Guid currentUserId, CancellationToken ct = default);
    Task<PagedResult<TaskSummaryDto>> SearchTasksAsync(TaskSearchQuery query, Guid currentUserId, CancellationToken ct = default);
    Task<IEnumerable<ReminderTaskDto>> GetTasksDueSoonAsync(TimeSpan window, CancellationToken ct = default);
}