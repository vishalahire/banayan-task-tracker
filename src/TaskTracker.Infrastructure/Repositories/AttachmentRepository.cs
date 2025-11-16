using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.Repositories;
using TaskTracker.Domain.Entities;
using TaskTracker.Infrastructure.Data;

namespace TaskTracker.Infrastructure.Repositories;

public class AttachmentRepository : IAttachmentRepository
{
    private readonly TaskTrackerDbContext _context;

    public AttachmentRepository(TaskTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<Attachment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Attachments
            .Include(a => a.Task)
            .Include(a => a.UploadedBy)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<IEnumerable<Attachment>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default)
    {
        return await _context.Attachments
            .Include(a => a.UploadedBy)
            .Where(a => a.TaskId == taskId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Guid> AddAsync(Attachment attachment, CancellationToken ct = default)
    {
        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync(ct);
        return attachment.Id;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var attachment = await _context.Attachments.FindAsync(new object[] { id }, ct);
        if (attachment != null)
        {
            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync(ct);
        }
    }
}