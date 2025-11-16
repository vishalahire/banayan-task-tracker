namespace TaskTracker.Domain.Entities;

public class Attachment
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public Guid TaskId { get; set; }
    public Guid UploadedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public TaskItem? Task { get; set; }
    public User? UploadedBy { get; set; }

    public Attachment()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Attachment(string fileName, string contentType, long fileSizeBytes, string storagePath, Guid taskId, Guid uploadedByUserId) : this()
    {
        FileName = fileName;
        ContentType = contentType;
        FileSizeBytes = fileSizeBytes;
        StoragePath = storagePath;
        TaskId = taskId;
        UploadedByUserId = uploadedByUserId;
    }

    public bool IsUploadedBy(Guid userId)
    {
        return UploadedByUserId == userId;
    }
}