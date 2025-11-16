using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.Repositories;
using TaskTracker.Domain.Entities;
using TaskTracker.Infrastructure.Data;

namespace TaskTracker.Infrastructure.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly TaskTrackerDbContext _context;

    public AuditRepository(TaskTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AuditEvent>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default)
    {
        return await _context.AuditEvents
            .Include(ae => ae.User)
            .Where(ae => ae.EntityId == taskId && ae.EntityType == nameof(Domain.Entities.TaskItem))
            .OrderByDescending(ae => ae.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<AuditEvent>> GetByUserIdAsync(Guid userId, int maxRecords, CancellationToken ct = default)
    {
        return await _context.AuditEvents
            .Include(ae => ae.User)
            .Where(ae => ae.UserId == userId)
            .OrderByDescending(ae => ae.CreatedAt)
            .Take(maxRecords)
            .ToListAsync(ct);
    }

    public async Task AddAsync(AuditEvent auditEvent, CancellationToken ct = default)
    {
        _context.AuditEvents.Add(auditEvent);
        await _context.SaveChangesAsync(ct);
    }
}