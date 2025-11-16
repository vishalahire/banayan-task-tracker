using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.Repositories;

public interface ITaskRepository
{
    System.Threading.Tasks.Task<Domain.Entities.TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    System.Threading.Tasks.Task<Domain.Entities.TaskItem?> GetByIdWithOwnerAsync(Guid id, CancellationToken ct = default);
    System.Threading.Tasks.Task<IEnumerable<Domain.Entities.TaskItem>> SearchAsync(
        string? searchText,
        TaskState? status,
        TaskPriority? priority,
        DateTimeOffset? dueDateFrom,
        DateTimeOffset? dueDateTo,
        IEnumerable<string>? tags,
        string sortBy,
        bool sortDescending,
        int skip,
        int take,
        CancellationToken ct = default);
    Task<int> CountAsync(
        string? searchText,
        TaskState? status,
        TaskPriority? priority,
        DateTimeOffset? dueDateFrom,
        DateTimeOffset? dueDateTo,
        IEnumerable<string>? tags,
        CancellationToken ct = default);
    System.Threading.Tasks.Task<IEnumerable<Domain.Entities.TaskItem>> GetTasksDueInWindowAsync(TimeSpan window, CancellationToken ct = default);
    System.Threading.Tasks.Task<Guid> AddAsync(Domain.Entities.TaskItem task, CancellationToken ct = default);
    System.Threading.Tasks.Task UpdateAsync(Domain.Entities.TaskItem task, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}