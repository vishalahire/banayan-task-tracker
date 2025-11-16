using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Repositories;

public interface IAttachmentRepository
{
    Task<Attachment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Attachment>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default);
    Task<Guid> AddAsync(Attachment attachment, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}