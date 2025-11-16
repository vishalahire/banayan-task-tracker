using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskTracker.Api.DTOs;
using TaskTracker.Application.Commands;
using TaskTracker.Application.Interfaces;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/tasks/{taskId:guid}/attachments")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;
    private readonly ILogger<AttachmentsController> _logger;

    public AttachmentsController(IAttachmentService attachmentService, ILogger<AttachmentsController> logger)
    {
        _attachmentService = attachmentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all attachments for a task
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AttachmentResponse>>> GetTaskAttachments(Guid taskId, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();

        try
        {
            var attachments = await _attachmentService.GetTaskAttachmentsAsync(taskId, currentUserId, ct);
            var response = attachments.Select(MapToAttachmentResponse);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Upload a new attachment to a task
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
    public async Task<ActionResult<AttachmentResponse>> UploadAttachment(
        Guid taskId,
        [FromForm] UploadAttachmentRequest request,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();

        // Validate file
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest("No file provided");
        }

        // Check file size (additional check beyond attribute)
        const long maxFileSize = 10 * 1024 * 1024; // 10MB
        if (request.File.Length > maxFileSize)
        {
            return BadRequest("File size exceeds 10MB limit");
        }

        // Check file type (basic validation)
        var allowedContentTypes = new[]
        {
            "application/pdf",
            "image/jpeg",
            "image/png", 
            "image/gif",
            "text/plain",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };

        if (!allowedContentTypes.Contains(request.File.ContentType.ToLowerInvariant()))
        {
            return BadRequest($"File type '{request.File.ContentType}' is not allowed");
        }

        var command = new UploadAttachmentCommand
        {
            TaskId = taskId,
            FileName = request.File.FileName,
            ContentType = request.File.ContentType,
            FileSizeBytes = request.File.Length,
            FileStream = request.File.OpenReadStream(),
            UploadedByUserId = currentUserId
        };

        try
        {
            var attachmentId = await _attachmentService.UploadAttachmentAsync(command, ct);
            
            // Get the created attachment details
            var attachments = await _attachmentService.GetTaskAttachmentsAsync(taskId, currentUserId, ct);
            var createdAttachment = attachments.FirstOrDefault(a => a.Id == attachmentId);
            
            if (createdAttachment == null)
            {
                return Problem("Attachment was uploaded but could not be retrieved");
            }

            var response = MapToAttachmentResponse(createdAttachment);
            return CreatedAtAction(nameof(GetAttachment), new { taskId, attachmentId }, response);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Download an attachment
    /// </summary>
    [HttpGet("{attachmentId:guid}")]
    public async Task<ActionResult> GetAttachment(Guid taskId, Guid attachmentId, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();

        try
        {
            var result = await _attachmentService.GetAttachmentAsync(attachmentId, currentUserId, ct);
            
            if (result == null)
            {
                return NotFound();
            }

            var (fileStream, fileName, contentType) = result.Value;
            
            return File(fileStream, contentType, fileName);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Delete an attachment
    /// </summary>
    [HttpDelete("{attachmentId:guid}")]
    public async Task<ActionResult> DeleteAttachment(Guid taskId, Guid attachmentId, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();

        try
        {
            await _attachmentService.DeleteAttachmentAsync(attachmentId, currentUserId, ct);
            return NoContent();
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

    private static AttachmentResponse MapToAttachmentResponse(Application.DTOs.AttachmentDto attachment)
    {
        return new AttachmentResponse
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            FileSizeBytes = attachment.FileSizeBytes,
            UploadedByUserId = attachment.UploadedByUserId,
            UploadedByDisplayName = attachment.UploadedByDisplayName,
            CreatedAt = attachment.CreatedAt
        };
    }
}