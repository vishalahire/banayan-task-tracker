using TaskTracker.Application.Commands;
using TaskTracker.Application.DTOs;

namespace TaskTracker.Application.Interfaces;

public interface IAttachmentService
{
    Task<Guid> UploadAttachmentAsync(UploadAttachmentCommand command, CancellationToken ct = default);
    Task<IEnumerable<AttachmentDto>> GetTaskAttachmentsAsync(Guid taskId, Guid currentUserId, CancellationToken ct = default);
    Task<(Stream FileStream, string FileName, string ContentType)?> GetAttachmentAsync(Guid attachmentId, Guid currentUserId, CancellationToken ct = default);
    Task DeleteAttachmentAsync(Guid attachmentId, Guid currentUserId, CancellationToken ct = default);
}