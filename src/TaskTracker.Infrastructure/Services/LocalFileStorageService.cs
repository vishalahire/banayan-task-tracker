using TaskTracker.Application.Services;

namespace TaskTracker.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storagePath;

    public LocalFileStorageService(string? storagePath = null)
    {
        _storagePath = storagePath ?? Path.Combine(Environment.CurrentDirectory, "uploads");
        EnsureStorageDirectoryExists();
    }

    public async Task<string> StoreFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        // Generate unique filename to avoid conflicts
        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var fullPath = Path.Combine(_storagePath, uniqueFileName);

        using var fileStreamOut = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await fileStream.CopyToAsync(fileStreamOut, ct);

        return uniqueFileName; // Return relative path for storage
    }

    public async Task<Stream> GetFileAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_storagePath, storagePath);
        
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {storagePath}");
        }

        var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return await Task.FromResult(fileStream);
    }

    public async Task DeleteFileAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_storagePath, storagePath);
        
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        await Task.CompletedTask;
    }

    private void EnsureStorageDirectoryExists()
    {
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }
}