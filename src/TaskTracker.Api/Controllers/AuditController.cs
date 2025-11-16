using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskTracker.Api.DTOs;
using TaskTracker.Application.Interfaces;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/tasks/{taskId:guid}/audit-events")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(IAuditService auditService, ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Get audit events for a specific task
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditEventResponse>>> GetTaskAuditEvents(Guid taskId, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();

        try
        {
            var auditEvents = await _auditService.GetTaskAuditEventsAsync(taskId, currentUserId, ct);
            var response = auditEvents.Select(MapToAuditEventResponse);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }

        return userId;
    }

    private static AuditEventResponse MapToAuditEventResponse(Application.DTOs.AuditEventDto auditEvent)
    {
        return new AuditEventResponse
        {
            Id = auditEvent.Id,
            Action = auditEvent.Action.ToString(),
            UserId = auditEvent.UserId,
            UserDisplayName = auditEvent.UserDisplayName,
            EntityId = auditEvent.EntityId,
            EntityType = auditEvent.EntityType,
            Details = auditEvent.Details,
            CreatedAt = auditEvent.CreatedAt
        };
    }
}

/// <summary>
/// User audit events controller (separate from task-specific audit)
/// </summary>
[ApiController]
[Route("api/audit")]
[Authorize]
public class UserAuditController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<UserAuditController> _logger;

    public UserAuditController(IAuditService auditService, ILogger<UserAuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Get audit events for the current user
    /// </summary>
    [HttpGet("my-events")]
    public async Task<ActionResult<IEnumerable<AuditEventResponse>>> GetMyAuditEvents(
        [FromQuery] int maxRecords = 100,
        CancellationToken ct = default)
    {
        var currentUserId = GetCurrentUserId();
        
        // Limit maxRecords to prevent excessive queries
        maxRecords = Math.Max(1, Math.Min(1000, maxRecords));

        var auditEvents = await _auditService.GetUserAuditEventsAsync(currentUserId, maxRecords, ct);
        var response = auditEvents.Select(MapToAuditEventResponse);
        
        return Ok(response);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }

        return userId;
    }

    private static AuditEventResponse MapToAuditEventResponse(Application.DTOs.AuditEventDto auditEvent)
    {
        return new AuditEventResponse
        {
            Id = auditEvent.Id,
            Action = auditEvent.Action.ToString(),
            UserId = auditEvent.UserId,
            UserDisplayName = auditEvent.UserDisplayName,
            EntityId = auditEvent.EntityId,
            EntityType = auditEvent.EntityType,
            Details = auditEvent.Details,
            CreatedAt = auditEvent.CreatedAt
        };
    }
}