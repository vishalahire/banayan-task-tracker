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

    public async Task<IReadOnlyList<PendingReminderDto>> GetPendingRemindersAsync(CancellationToken ct = default)
    {
        var window = TimeSpan.FromHours(24);
        var tasksDue = await _taskRepository.GetTasksDueInWindowAsync(window, ct);
        var pendingReminders = new List<PendingReminderDto>();

        foreach (var task in tasksDue)
        {
            if (!task.DueDate.HasValue) continue;

            var timeUntilDue = task.DueDate.Value - DateTimeOffset.UtcNow;
            var reminderType = DetermineReminderType(timeUntilDue);
            
            var hasReminderBeenSent = await _reminderLogRepository.HasReminderBeenSentAsync(
                task.Id, reminderType, task.DueDate.Value, ct);

            pendingReminders.Add(new PendingReminderDto
            {
                TaskId = task.Id,
                TaskTitle = task.Title,
                DueDate = task.DueDate.Value,
                OwnerUserId = task.OwnerUserId,
                OwnerEmail = task.Owner?.Email ?? "unknown@example.com",
                OwnerDisplayName = task.Owner?.DisplayName ?? "Unknown",
                ReminderType = reminderType,
                HasReminderBeenSent = hasReminderBeenSent,
                TimeUntilDue = timeUntilDue
            });
        }

        return pendingReminders;
    }

    public async Task<ReminderProcessingResultDto> ProcessPendingRemindersAsync(CancellationToken ct = default)
    {
        var pendingReminders = await GetPendingRemindersAsync(ct);
        var result = new ReminderProcessingResultDto
        {
            TotalPending = pendingReminders.Count
        };

        foreach (var pending in pendingReminders.Where(p => !p.HasReminderBeenSent))
        {
            try
            {
                // Simulate sending reminder (95% success rate for demo purposes)
                var deliverySuccessful = SimulateReminderDelivery();
                var deliveryDetails = deliverySuccessful ? "Reminder sent successfully" : "Failed to send reminder";

                // Log the reminder attempt
                var logSuccess = await LogReminderSentAsync(
                    pending.TaskId,
                    pending.ReminderType,
                    deliverySuccessful,
                    deliveryDetails,
                    ct);

                if (logSuccess)
                {
                    result.ProcessedCount++;
                    result.ProcessedTaskIds.Add(pending.TaskId);
                    
                    if (deliverySuccessful)
                    {
                        result.SuccessfulCount++;
                    }
                    else
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Failed to deliver reminder for task '{pending.TaskTitle}'");
                    }
                }
                else
                {
                    result.FailedCount++;
                    result.Errors.Add($"Failed to log reminder for task '{pending.TaskTitle}'");
                }
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.Errors.Add($"Error processing reminder for task '{pending.TaskTitle}': {ex.Message}");
            }
        }

        result.SkippedCount = result.TotalPending - result.ProcessedCount;
        return result;
    }

    private static string DetermineReminderType(TimeSpan timeUntilDue)
    {
        return timeUntilDue.TotalHours switch
        {
            <= 1 => "1Hour",
            <= 4 => "4Hours",
            <= 24 => "24Hours",
            _ => "24Hours" // Default fallback
        };
    }

    private static bool SimulateReminderDelivery()
    {
        // Simulate 95% success rate for demo purposes
        var random = new Random();
        return random.NextDouble() < 0.95;
    }
}