using FileStorageService.Data;
using FileStorageService.Models;
using Microsoft.EntityFrameworkCore;

namespace FileStorageService.Services;

public class FileStorageServiceImpl : IFileStorageService
{
    private readonly FileStorageDbContext _context;
    private readonly string _storagePath;
    private readonly ILogger<FileStorageServiceImpl> _logger;

    public FileStorageServiceImpl(FileStorageDbContext context, IConfiguration configuration, ILogger<FileStorageServiceImpl> logger)
    {
        _context = context;
        _logger = logger;
        _storagePath = configuration["FileStorage:StoragePath"] ?? "/app/storage";
        
        // Ensure storage directory exists
        Directory.CreateDirectory(_storagePath);
        _logger.LogInformation("File storage path initialized: {StoragePath}", _storagePath);
    }

    public async Task<(bool Success, string Message, Guid? FileId)> StoreFileAsync(IFormFile file, string uploadedBy, string? description = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return (false, "File is empty", null);
            }

            // TODO move this to config
            const long maxFileSize = 100 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                return (false, $"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)}MB", null);
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_storagePath, fileName);

            // Save file to disk
            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Create metadata record
            var fileMetadata = new FileMetadata(
                fileName: fileName,
                originalFileName: file.FileName,
                contentType: file.ContentType,
                size: file.Length,
                filePath: filePath,
                uploadedBy: uploadedBy,
                description: description
            );

            _context.Files.Add(fileMetadata);
            await _context.SaveChangesAsync();

            _logger.LogInformation("File stored successfully: {FileName} ({FileSize} bytes) by {UploadedBy}", 
                file.FileName, file.Length, uploadedBy);

            return (true, "File stored successfully", fileMetadata.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing file: {FileName}", file?.FileName ?? "unknown");
            return (false, $"Error storing file: {ex.Message}", null);
        }
    }

    public async Task<(byte[]? Content, string? ContentType, string? FileName)> RetrieveFileAsync(Guid fileId)
    {
        try
        {
            var fileMetadata = await _context.Files
                .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);

            if (fileMetadata == null)
            {
                return (null, null, null);
            }

            if (!File.Exists(fileMetadata.FilePath))
            {
                _logger.LogWarning("File not found on disk: {FilePath}", fileMetadata.FilePath);
                return (null, null, null);
            }

            var content = await File.ReadAllBytesAsync(fileMetadata.FilePath);
            
            _logger.LogInformation("File retrieved: {FileName} ({FileSize} bytes)", 
                fileMetadata.OriginalFileName, content.Length);

            return (content, fileMetadata.ContentType, fileMetadata.OriginalFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file: {FileId}", fileId);
            return (null, null, null);
        }
    }

    public async Task<bool> DeleteFileAsync(Guid fileId, string deletedBy)
    {
        try
        {
            var fileMetadata = await _context.Files
                .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);

            if (fileMetadata == null)
            {
                return false;
            }

            // Mark as deleted in database
            fileMetadata.IsDeleted = true;
            fileMetadata.DeletedAt = DateTime.UtcNow;
            fileMetadata.DeletedBy = deletedBy;
            
            await _context.SaveChangesAsync();

            // Delete physical file
            if (File.Exists(fileMetadata.FilePath))
            {
                File.Delete(fileMetadata.FilePath);
                _logger.LogInformation("Physical file deleted: {FilePath}", fileMetadata.FilePath);
            }

            _logger.LogInformation("File marked as deleted: {FileName} by {DeletedBy}", 
                fileMetadata.OriginalFileName, deletedBy);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileId}", fileId);
            return false;
        }
    }

    public async Task<List<FileMetadata>> ListFilesAsync(string? uploadedBy = null, bool includeDeleted = false)
    {
        try
        {
            var query = _context.Files.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(f => !f.IsDeleted);
            }

            if (!string.IsNullOrEmpty(uploadedBy))
            {
                query = query.Where(f => f.UploadedBy == uploadedBy);
            }

            var files = await query
                .OrderByDescending(f => f.UploadDate)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} files", files.Count);
            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files");
            return new List<FileMetadata>();
        }
    }

    public async Task<FileMetadata?> GetFileMetadataAsync(Guid fileId)
    {
        try
        {
            return await _context.Files
                .FirstOrDefaultAsync(f => f.Id == fileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file metadata: {FileId}", fileId);
            return null;
        }
    }
}
