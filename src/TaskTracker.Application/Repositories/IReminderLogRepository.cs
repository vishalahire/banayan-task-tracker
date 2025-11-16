using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Repositories;

public interface IReminderLogRepository
{
    Task<bool> HasReminderBeenSentAsync(Guid taskId, string reminderType, DateTimeOffset taskDueDate, CancellationToken ct = default);
    Task<Guid> AddAsync(ReminderLog reminderLog, CancellationToken ct = default);
    Task UpdateAsync(ReminderLog reminderLog, CancellationToken ct = default);
}