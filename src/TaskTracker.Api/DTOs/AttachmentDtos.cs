using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Api.DTOs;

public class AttachmentResponse
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public Guid UploadedByUserId { get; set; }
    public string UploadedByDisplayName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public class UploadAttachmentRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;
}