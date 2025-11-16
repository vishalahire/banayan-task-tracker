using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Api.DTOs;
using TaskTracker.Application.Interfaces;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RemindersController : ControllerBase
{
    private readonly IReminderService _reminderService;
    private readonly ILogger<RemindersController> _logger;

    public RemindersController(IReminderService reminderService, ILogger<RemindersController> logger)
    {
        _reminderService = reminderService;
        _logger = logger;
    }

    /// <summary>
    /// Get pending reminders (tasks due in the next 24 hours without logged reminders)
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<List<PendingReminderResponse>>> GetPendingReminders(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Fetching pending reminders");
            
            var pendingReminders = await _reminderService.GetPendingRemindersAsync(ct);
            
            var response = pendingReminders.Select(MapToPendingReminderResponse).ToList();
            
            _logger.LogInformation("Found {Count} pending reminders", response.Count);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending reminders");
            return StatusCode(500, "An error occurred while fetching pending reminders");
        }
    }

    /// <summary>
    /// Process all pending reminders on-demand
    /// </summary>
    [HttpPost("process")]
    public async Task<ActionResult<ReminderProcessingResultResponse>> ProcessPendingReminders(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Processing pending reminders on-demand");
            
            var result = await _reminderService.ProcessPendingRemindersAsync(ct);
            
            var response = MapToReminderProcessingResultResponse(result);
            
            _logger.LogInformation("Processed {ProcessedCount} reminders. Success: {SuccessCount}, Failed: {FailCount}, Skipped: {SkippedCount}",
                response.ProcessedCount, response.SuccessfulCount, response.FailedCount, response.SkippedCount);
                
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing pending reminders");
            return StatusCode(500, "An error occurred while processing reminders");
        }
    }

    private static PendingReminderResponse MapToPendingReminderResponse(Application.DTOs.PendingReminderDto dto)
    {
        return new PendingReminderResponse
        {
            TaskId = dto.TaskId,
            TaskTitle = dto.TaskTitle,
            DueDate = dto.DueDate,
            OwnerUserId = dto.OwnerUserId,
            OwnerEmail = dto.OwnerEmail,
            OwnerDisplayName = dto.OwnerDisplayName,
            ReminderType = dto.ReminderType,
            HasReminderBeenSent = dto.HasReminderBeenSent,
            HoursUntilDue = dto.TimeUntilDue.TotalHours
        };
    }

    private static ReminderProcessingResultResponse MapToReminderProcessingResultResponse(Application.DTOs.ReminderProcessingResultDto dto)
    {
        return new ReminderProcessingResultResponse
        {
            TotalPending = dto.TotalPending,
            ProcessedCount = dto.ProcessedCount,
            SuccessfulCount = dto.SuccessfulCount,
            FailedCount = dto.FailedCount,
            SkippedCount = dto.SkippedCount,
            ProcessedTaskIds = dto.ProcessedTaskIds,
            Errors = dto.Errors
        };
    }
}