using Microsoft.AspNetCore.Mvc;
using FileStorageService.Services;
using FileStorageService.Models;
using System.ComponentModel.DataAnnotations;

namespace FileStorageService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController(IFileStorageService fileStorageService, ILogger<FilesController> logger)
    : ControllerBase
{
    /// <summary>
    /// Upload a file to the storage service
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/files/upload
    ///     Content-Type: multipart/form-data
    ///     
    ///     file: [binary file content]
    ///     description: "Document description"
    ///     
    /// Headers:
    ///     X-Uploaded-By: username (optional)
    /// </remarks>
    /// <param name="file">The file to upload (max 100MB)</param>
    /// <param name="description">Optional description of the file</param>
    /// <returns>File metadata including the assigned ID</returns>
    /// <response code="200">File uploaded successfully</response>
    /// <response code="400">Invalid file or request</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadFile([FromForm] FileUploadRequest request)
    {
        try
        {
            var uploadedBy = Request.Headers["X-Uploaded-By"].FirstOrDefault() ?? "anonymous";

            var (success, message, fileId) = await fileStorageService.StoreFileAsync(
                request.File,
                uploadedBy,
                request.Description);

            if (!success)
            {
                return BadRequest(new { Message = message });
            }

            return Ok(new {
                Message = message,
                FileId = fileId,
                FileName = request.File.FileName,
                Size = request.File.Length,
                ContentType = request.File.ContentType
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading file");
            return StatusCode(500, new { Message = "Internal server error occurred while uploading file" });
        }
    }

    /// <summary>
    /// Download a file by its unique identifier
    /// </summary>
    /// <param name="id">The unique file identifier (GUID)</param>
    /// <returns>The file content with appropriate content type and filename</returns>
    /// <response code="200">File content retrieved successfully</response>
    /// <response code="404">File not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadFile([FromRoute] Guid id)
    {
        try
        {
            var (content, contentType, fileName) = await fileStorageService.RetrieveFileAsync(id);

            if (content == null || contentType == null || fileName == null)
            {
                return NotFound(new { Message = "File not found" });
            }

            return File(content, contentType, fileName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error downloading file: {FileId}", id);
            return StatusCode(500, new { Message = "Internal server error occurred while downloading file" });
        }
    }

    /// <summary>
    /// Get metadata for a specific file
    /// </summary>
    /// <param name="id">The unique file identifier (GUID)</param>
    /// <returns>Complete file metadata without sensitive internal information</returns>
    /// <response code="200">File metadata retrieved successfully</response>
    /// <response code="404">File not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}/metadata")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFileMetadata([FromRoute] Guid id)
    {
        try
        {
            var metadata = await fileStorageService.GetFileMetadataAsync(id);

            if (metadata == null)
            {
                return NotFound(new { Message = "File not found" });
            }

            // Return metadata without sensitive information
            return Ok(new
            {
                Id = metadata.Id,
                FileName = metadata.OriginalFileName,
                ContentType = metadata.ContentType,
                Size = metadata.Size,
                UploadDate = metadata.UploadDate,
                UploadedBy = metadata.UploadedBy,
                Description = metadata.Description,
                IsDeleted = metadata.IsDeleted
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting file metadata: {FileId}", id);
            return StatusCode(500, new { Message = "Internal server error occurred while retrieving file metadata" });
        }
    }

    /// <summary>
    /// Delete a file by marking it as deleted (soft delete)
    /// </summary>
    /// <remarks>
    /// This performs a soft delete - the file metadata remains in the database
    /// but the file is marked as deleted and the physical file is removed.
    /// 
    /// Headers:
    ///     X-Deleted-By: username (optional)
    /// </remarks>
    /// <param name="id">The unique file identifier (GUID)</param>
    /// <returns>Confirmation of successful deletion</returns>
    /// <response code="200">File deleted successfully</response>
    /// <response code="404">File not found or already deleted</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteFile([FromRoute] Guid id)
    {
        try
        {
            var deletedBy = Request.Headers["X-Deleted-By"].FirstOrDefault() ?? "anonymous";

            var success = await fileStorageService.DeleteFileAsync(id, deletedBy);

            if (!success) { return NotFound(new { Message = "File not found or already deleted" }); }

            return Ok(new { Message = "File deleted successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting file: {FileId}", id);
            return StatusCode(500, new { Message = "Internal server error occurred while deleting file" });
        }
    }

    /// <summary>
    /// List all files with optional filtering
    /// </summary>
    /// <remarks>
    /// Returns a list of files with their metadata. By default, only active (non-deleted)
    /// files are returned. Use query parameters to filter results.
    /// </remarks>
    /// <param name="uploadedBy">Filter files by uploader username (optional)</param>
    /// <param name="includeDeleted">Include deleted files in results (default: false)</param>
    /// <returns>List of file metadata objects</returns>
    /// <response code="200">Files list retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListFiles(
        [FromQuery] string? uploadedBy = null, 
        [FromQuery] bool includeDeleted = false)
    {
        try
        {
            var files = await fileStorageService.ListFilesAsync(uploadedBy, includeDeleted);

            var result = files.Select(f => new
            {
                Id = f.Id,
                FileName = f.OriginalFileName,
                ContentType = f.ContentType,
                Size = f.Size,
                UploadDate = f.UploadDate,
                UploadedBy = f.UploadedBy,
                Description = f.Description,
                IsDeleted = f.IsDeleted
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing files");
            return StatusCode(500, new { Message = "Internal server error occurred while listing files" });
        }
    }
}
