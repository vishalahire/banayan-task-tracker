using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.Interfaces;

public interface IAuditService
{
    Task RecordAuditEventAsync(AuditAction action, Guid userId, Guid? entityId, string entityType, string details, CancellationToken ct = default);
    Task<IEnumerable<AuditEventDto>> GetTaskAuditEventsAsync(Guid taskId, Guid currentUserId, CancellationToken ct = default);
    Task<IEnumerable<AuditEventDto>> GetUserAuditEventsAsync(Guid userId, int maxRecords = 100, CancellationToken ct = default);
}