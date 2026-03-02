using FileStorageService.Models;

namespace FileStorageService.Services;

public interface IFileStorageService
{
    Task<(bool Success, string Message, Guid? FileId)> StoreFileAsync(IFormFile file, string uploadedBy, string? description = null);
    Task<(byte[]? Content, string? ContentType, string? FileName)> RetrieveFileAsync(Guid fileId);
    Task<bool> DeleteFileAsync(Guid fileId, string deletedBy);
    Task<List<FileMetadata>> ListFilesAsync(string? uploadedBy = null, bool includeDeleted = false);
    Task<FileMetadata?> GetFileMetadataAsync(Guid fileId);
}