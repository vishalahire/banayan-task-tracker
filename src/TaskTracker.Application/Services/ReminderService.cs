using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Repositories;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Services;

public class ReminderService : IReminderService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IReminderLogRepository _reminderLogRepository;
    private readonly IAuditRepository _auditRepository;

    public ReminderService(
        ITaskRepository taskRepository,
        IReminderLogRepository reminderLogRepository,
        IAuditRepository auditRepository)
    {
        _taskRepository = taskRepository;
        _reminderLogRepository = reminderLogRepository;
        _auditRepository = auditRepository;
    }

    public async Task<IEnumerable<ReminderTaskDto>> GetTasksDueForReminderAsync(TimeSpan window, CancellationToken ct = default)
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

    public async Task<bool> LogReminderSentAsync(Guid taskId, string reminderType, bool deliverySuccessful, string? deliveryDetails = null, CancellationToken ct = default)
    {
        // Get the task to ensure it exists and get due date
        var task = await _taskRepository.GetByIdWithOwnerAsync(taskId, ct);
        if (task == null || !task.DueDate.HasValue)
        {
            return false;
        }

        // Check if reminder has already been sent (idempotency check)
        var alreadySent = await _reminderLogRepository.HasReminderBeenSentAsync(taskId, reminderType, task.DueDate.Value, ct);
        if (alreadySent)
        {
            return true; // Already sent, return success
        }

        // Create reminder log entry
        var reminderLog = new ReminderLog(taskId, task.OwnerUserId, task.DueDate.Value, reminderType);
        reminderLog.MarkDelivered(deliverySuccessful, deliveryDetails);

        await _reminderLogRepository.AddAsync(reminderLog, ct);

        // Record audit event if delivery was successful
        if (deliverySuccessful)
        {
            var auditEvent = AuditEvent.ReminderSent(task.OwnerUserId, taskId, task.Title);
            await _auditRepository.AddAsync(auditEvent, ct);
        }

        return true;
    }

    public async Task<bool> HasReminderBeenSentAsync(Guid taskId, string reminderType, DateTimeOffset taskDueDate, CancellationToken ct = default)
    {
        return await _reminderLogRepository.HasReminderBeenSentAsync(taskId, reminderType, taskDueDate, ct);
    }
}