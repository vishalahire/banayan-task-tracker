using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.Repositories;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Infrastructure.Data;

namespace TaskTracker.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly TaskTrackerDbContext _context;

    public TaskRepository(TaskTrackerDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<Domain.Entities.TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Tasks
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async System.Threading.Tasks.Task<Domain.Entities.TaskItem?> GetByIdWithOwnerAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Tasks
            .Include(t => t.Owner)
            .Include(t => t.Attachments)
                .ThenInclude(a => a.UploadedBy)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Domain.Entities.TaskItem>> SearchAsync(
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
        CancellationToken ct = default)
    {
        var query = _context.Tasks
            .Include(t => t.Owner)
            .AsQueryable();

        // Apply filters
        query = ApplyFilters(query, searchText, status, priority, dueDateFrom, dueDateTo, tags);

        // Apply sorting
        query = ApplySorting(query, sortBy, sortDescending);

        // Apply pagination
        query = query.Skip(skip).Take(take);

        return await query.ToListAsync(ct);
    }

    public async Task<int> CountAsync(
        string? searchText,
        TaskState? status,
        TaskPriority? priority,
        DateTimeOffset? dueDateFrom,
        DateTimeOffset? dueDateTo,
        IEnumerable<string>? tags,
        CancellationToken ct = default)
    {
        var query = _context.Tasks.AsQueryable();
        query = ApplyFilters(query, searchText, status, priority, dueDateFrom, dueDateTo, tags);
        return await query.CountAsync(ct);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Domain.Entities.TaskItem>> GetTasksDueInWindowAsync(TimeSpan window, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var windowEnd = now.Add(window);

        return await _context.Tasks
            .Include(t => t.Owner)
            .Where(t => t.DueDate.HasValue
                       && t.DueDate >= now
                       && t.DueDate <= windowEnd
                       && t.Status != TaskState.Completed
                       && t.Status != TaskState.Archived)
            .ToListAsync(ct);
    }

    public async System.Threading.Tasks.Task<Guid> AddAsync(Domain.Entities.TaskItem task, CancellationToken ct = default)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(ct);
        return task.Id;
    }

    public async System.Threading.Tasks.Task UpdateAsync(Domain.Entities.TaskItem task, CancellationToken ct = default)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var task = await _context.Tasks.FindAsync(new object[] { id }, ct);
        if (task != null)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync(ct);
        }
    }

    private static IQueryable<Domain.Entities.TaskItem> ApplyFilters(
        IQueryable<Domain.Entities.TaskItem> query,
        string? searchText,
        TaskState? status,
        TaskPriority? priority,
        DateTimeOffset? dueDateFrom,
        DateTimeOffset? dueDateTo,
        IEnumerable<string>? tags)
    {
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(t => t.Title.Contains(searchText) || t.Description.Contains(searchText));
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority.Value);
        }

        if (dueDateFrom.HasValue)
        {
            query = query.Where(t => t.DueDate >= dueDateFrom.Value);
        }

        if (dueDateTo.HasValue)
        {
            query = query.Where(t => t.DueDate <= dueDateTo.Value);
        }

        if (tags != null && tags.Any())
        {
            // For PostgreSQL JSONB, we can use EF.Functions.JsonContains or similar
            // For simplicity, we'll check if any of the requested tags exist in the task's tags
            foreach (var tag in tags)
            {
                query = query.Where(t => t.Tags.Contains(tag));
            }
        }

        return query;
    }

    private static IQueryable<Domain.Entities.TaskItem> ApplySorting(
        IQueryable<Domain.Entities.TaskItem> query,
        string sortBy,
        bool sortDescending)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "title" => sortDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "status" => sortDescending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "priority" => sortDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "duedate" => sortDescending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
            "updatedat" => sortDescending ? query.OrderByDescending(t => t.UpdatedAt) : query.OrderBy(t => t.UpdatedAt),
            _ => sortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
        };
    }
}