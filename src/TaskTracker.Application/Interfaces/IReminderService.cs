using TaskTracker.Application.DTOs;

namespace TaskTracker.Application.Interfaces;

public interface IReminderService
{
    Task<IEnumerable<ReminderTaskDto>> GetTasksDueForReminderAsync(TimeSpan window, CancellationToken ct = default);
    Task<bool> LogReminderSentAsync(Guid taskId, string reminderType, bool deliverySuccessful, string? deliveryDetails = null, CancellationToken ct = default);
    Task<bool> HasReminderBeenSentAsync(Guid taskId, string reminderType, DateTimeOffset taskDueDate, CancellationToken ct = default);
    
    // New methods for on-demand reminder processing
    Task<IReadOnlyList<PendingReminderDto>> GetPendingRemindersAsync(CancellationToken ct = default);
    Task<ReminderProcessingResultDto> ProcessPendingRemindersAsync(CancellationToken ct = default);
}