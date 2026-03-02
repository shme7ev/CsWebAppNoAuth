namespace FileStorageService.Models;

public class FileMetadata
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public FileMetadata() { }

    public FileMetadata(string fileName, string originalFileName, string contentType,
                       long size, string filePath, string uploadedBy, string? description = null)
    {
        Id = Guid.NewGuid();
        FileName = fileName;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        Size = size;
        FilePath = filePath;
        UploadDate = DateTime.UtcNow;
        UploadedBy = uploadedBy;
        Description = description;
        IsDeleted = false;
    }
}
