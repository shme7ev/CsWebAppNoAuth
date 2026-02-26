using Microsoft.AspNetCore.Mvc;
using WebAppNoAuth.Services;
using Microsoft.Extensions.Logging;

namespace WebAppNoAuth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> GetUser(string username)
    {
        _logger.LogDebug("UserController.GetUser() called with username: {Username}", username);
        
        var user = await _userService.GetUserByUsernameAsync(username);
        
        if (user == null)
        {
            _logger.LogWarning("User not found: {Username}", username);
            return NotFound($"User '{username}' not found");
        }
        
        _logger.LogDebug("User found: {Username}, Role: {Role}", username, user.Role);
        return Ok(user);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        _logger.LogDebug("UserController.GetAllUsers() called");
        
        var users = await _userService.GetAllUsersAsync();
        _logger.LogDebug("Retrieved {Count} users from database", users.Count);
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] Models.AuthUser user)
    {
        _logger.LogDebug("UserController.CreateUser() called with user: {@User}", user);
        
        if (string.IsNullOrWhiteSpace(user.Username))
        {
            _logger.LogWarning("CreateUser called with invalid user data");
            return BadRequest("Invalid user data");
        }

        _logger.LogDebug("Creating new user: {Username}, Role: {Role}", user.Username, user.Role);
        var result = await _userService.CreateUserAsync(user);
        
        if (result)
        {
            _logger.LogDebug("User created successfully: {Username}", user.Username);
            return CreatedAtAction(nameof(GetUser), new { username = user.Username }, user);
        }
        
        _logger.LogWarning("User creation failed - user already exists: {Username}", user.Username);
        return Conflict($"User '{user.Username}' already exists");
    }
}
