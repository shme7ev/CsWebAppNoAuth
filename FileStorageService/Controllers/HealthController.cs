using Microsoft.AspNetCore.Mvc;

namespace FileStorageService.Controllers;

/// <summary>
/// Health check endpoint for monitoring service status
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Check the health status of the file storage service
    /// </summary>
    /// <returns>Health status information including service name and timestamp</returns>
    /// <response code="200">Service is healthy and operational</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        _logger.LogDebug("Health check requested");
        return Ok(new { Status = "Healthy", Service = "FileStorageService", Timestamp = DateTime.UtcNow });
    }
}