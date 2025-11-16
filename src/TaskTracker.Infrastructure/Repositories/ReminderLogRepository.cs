using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.Repositories;
using TaskTracker.Domain.Entities;
using TaskTracker.Infrastructure.Data;

namespace TaskTracker.Infrastructure.Repositories;

public class ReminderLogRepository : IReminderLogRepository
{
    private readonly TaskTrackerDbContext _context;

    public ReminderLogRepository(TaskTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasReminderBeenSentAsync(Guid taskId, string reminderType, DateTimeOffset taskDueDate, CancellationToken ct = default)
    {
        return await _context.ReminderLogs
            .AnyAsync(rl => rl.TaskId == taskId 
                           && rl.ReminderType == reminderType 
                           && rl.TaskDueDate == taskDueDate, ct);
    }

    public async Task<Guid> AddAsync(ReminderLog reminderLog, CancellationToken ct = default)
    {
        try
        {
            _context.ReminderLogs.Add(reminderLog);
            await _context.SaveChangesAsync(ct);
            return reminderLog.Id;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Idempotency: reminder already exists for this task/type/due date combination
            // Return the ID of the existing reminder
            var existing = await _context.ReminderLogs
                .FirstOrDefaultAsync(rl => rl.TaskId == reminderLog.TaskId 
                                          && rl.ReminderType == reminderLog.ReminderType 
                                          && rl.TaskDueDate == reminderLog.TaskDueDate, ct);
            return existing?.Id ?? reminderLog.Id;
        }
    }

    public async Task UpdateAsync(ReminderLog reminderLog, CancellationToken ct = default)
    {
        _context.ReminderLogs.Update(reminderLog);
        await _context.SaveChangesAsync(ct);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        // Check for PostgreSQL unique constraint violation
        return ex.InnerException?.Message?.Contains("ix_reminder_logs_unique_reminder") == true;
    }
}