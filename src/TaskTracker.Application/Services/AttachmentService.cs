using TaskTracker.Application.Commands;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Repositories;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Services;

public class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IFileStorageService _fileStorageService;

    public AttachmentService(
        IAttachmentRepository attachmentRepository,
        ITaskRepository taskRepository,
        IAuditRepository auditRepository,
        IFileStorageService fileStorageService)
    {
        _attachmentRepository = attachmentRepository;
        _taskRepository = taskRepository;
        _auditRepository = auditRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Guid> UploadAttachmentAsync(UploadAttachmentCommand command, CancellationToken ct = default)
    {
        // Verify task exists and user has access
        var task = await _taskRepository.GetByIdAsync(command.TaskId, ct);
        if (task == null)
        {
            throw new InvalidOperationException("Task not found");
        }

        // Check if user can upload to this task (only task owner for now)
        if (!task.IsOwnedBy(command.UploadedByUserId))
        {
            throw new UnauthorizedAccessException("You can only upload attachments to your own tasks");
        }

        // Store the file
        var storagePath = await _fileStorageService.StoreFileAsync(
            command.FileStream, 
            command.FileName, 
            command.ContentType, 
            ct);

        // Create attachment record
        var attachment = new Attachment(
            command.FileName,
            command.ContentType,
            command.FileSizeBytes,
            storagePath,
            command.TaskId,
            command.UploadedByUserId);

        var attachmentId = await _attachmentRepository.AddAsync(attachment, ct);

        // Record audit event
        var auditEvent = AuditEvent.AttachmentAdded(command.UploadedByUserId, attachmentId, command.FileName, command.TaskId);
        await _auditRepository.AddAsync(auditEvent, ct);

        return attachmentId;
    }

    public async Task<IEnumerable<AttachmentDto>> GetTaskAttachmentsAsync(Guid taskId, Guid currentUserId, CancellationToken ct = default)
    {
        // Verify task exists and user has access
        var task = await _taskRepository.GetByIdAsync(taskId, ct);
        if (task == null)
        {
            throw new InvalidOperationException("Task not found");
        }

        // Users can view attachments for any task (as per requirements)
        var attachments = await _attachmentRepository.GetByTaskIdAsync(taskId, ct);

        return attachments.Select(a => new AttachmentDto
        {
            Id = a.Id,
            FileName = a.FileName,
            ContentType = a.ContentType,
            FileSizeBytes = a.FileSizeBytes,
            UploadedByUserId = a.UploadedByUserId,
            UploadedByDisplayName = a.UploadedBy?.DisplayName ?? "Unknown",
            CreatedAt = a.CreatedAt
        });
    }

    public async Task<(Stream FileStream, string FileName, string ContentType)?> GetAttachmentAsync(Guid attachmentId, Guid currentUserId, CancellationToken ct = default)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId, ct);
        if (attachment == null)
        {
            return null;
        }

        // Users can download attachments from any task (as per requirements)
        var fileStream = await _fileStorageService.GetFileAsync(attachment.StoragePath, ct);
        
        return (fileStream, attachment.FileName, attachment.ContentType);
    }

    public async Task DeleteAttachmentAsync(Guid attachmentId, Guid currentUserId, CancellationToken ct = default)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId, ct);
        if (attachment == null)
        {
            throw new InvalidOperationException("Attachment not found");
        }

        // Check if user can delete this attachment (task owner or uploader)
        if (!attachment.Task!.IsOwnedBy(currentUserId) && !attachment.IsUploadedBy(currentUserId))
        {
            throw new UnauthorizedAccessException("You can only delete attachments from your own tasks or attachments you uploaded");
        }

        // Delete the file from storage
        await _fileStorageService.DeleteFileAsync(attachment.StoragePath, ct);

        // Delete the attachment record
        await _attachmentRepository.DeleteAsync(attachmentId, ct);

        // Record audit event
        var auditEvent = AuditEvent.AttachmentRemoved(currentUserId, attachmentId, attachment.FileName, attachment.TaskId);
        await _auditRepository.AddAsync(auditEvent, ct);
    }
}

// File storage interface - will be implemented in Infrastructure
public interface IFileStorageService
{
    Task<string> StoreFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default);
    Task<Stream> GetFileAsync(string storagePath, CancellationToken ct = default);
    Task DeleteFileAsync(string storagePath, CancellationToken ct = default);
}