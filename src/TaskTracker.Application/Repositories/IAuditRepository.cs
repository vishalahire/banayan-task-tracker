using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Repositories;

public interface IAuditRepository
{
    Task<IEnumerable<AuditEvent>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default);
    Task<IEnumerable<AuditEvent>> GetByUserIdAsync(Guid userId, int maxRecords, CancellationToken ct = default);
    Task AddAsync(AuditEvent auditEvent, CancellationToken ct = default);
}