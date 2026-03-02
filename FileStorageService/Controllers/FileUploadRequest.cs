namespace FileStorageService.Controllers;

public class FileUploadRequest
{
    public required IFormFile File { get; set; }

    public string? Description { get; set; }
}
