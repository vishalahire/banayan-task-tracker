using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Repositories;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;
    private readonly ITaskRepository _taskRepository;

    public AuditService(IAuditRepository auditRepository, ITaskRepository taskRepository)
    {
        _auditRepository = auditRepository;
        _taskRepository = taskRepository;
    }

    public async Task RecordAuditEventAsync(AuditAction action, Guid userId, Guid? entityId, string entityType, string details, CancellationToken ct = default)
    {
        var auditEvent = new AuditEvent(action, userId, entityId, entityType, details);
        await _auditRepository.AddAsync(auditEvent, ct);
    }

    public async Task<IEnumerable<AuditEventDto>> GetTaskAuditEventsAsync(Guid taskId, Guid currentUserId, CancellationToken ct = default)
    {
        // Verify task exists and user has access
        var task = await _taskRepository.GetByIdAsync(taskId, ct);
        if (task == null)
        {
            throw new InvalidOperationException("Task not found");
        }

        // Users can view audit events for any task (as per requirements)
        var auditEvents = await _auditRepository.GetByTaskIdAsync(taskId, ct);

        return auditEvents.Select(ae => new AuditEventDto
        {
            Id = ae.Id,
            Action = ae.Action,
            UserId = ae.UserId,
            UserDisplayName = ae.User?.DisplayName ?? "Unknown",
            EntityId = ae.EntityId,
            EntityType = ae.EntityType,
            Details = ae.Details,
            CreatedAt = ae.CreatedAt
        });
    }

    public async Task<IEnumerable<AuditEventDto>> GetUserAuditEventsAsync(Guid userId, int maxRecords = 100, CancellationToken ct = default)
    {
        var auditEvents = await _auditRepository.GetByUserIdAsync(userId, maxRecords, ct);

        return auditEvents.Select(ae => new AuditEventDto
        {
            Id = ae.Id,
            Action = ae.Action,
            UserId = ae.UserId,
            UserDisplayName = ae.User?.DisplayName ?? "Unknown",
            EntityId = ae.EntityId,
            EntityType = ae.EntityType,
            Details = ae.Details,
            CreatedAt = ae.CreatedAt
        });
    }
}