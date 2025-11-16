namespace TaskTracker.Application.Commands;

public class UploadAttachmentCommand
{
    public Guid TaskId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public Stream FileStream { get; set; } = Stream.Null;
    public Guid UploadedByUserId { get; set; }
}