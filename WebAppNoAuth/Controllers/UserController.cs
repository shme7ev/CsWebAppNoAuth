using Microsoft.AspNetCore.Mvc;
using WebAppNoAuth.Services;
using Microsoft.Extensions.Logging;

namespace WebAppNoAuth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService, ILogger<UserController> logger) : ControllerBase
{
    [HttpGet("{username}")]
    public async Task<IActionResult> GetUser(string username)
    {
        logger.LogDebug("UserController.GetUser() called with username: {Username}", username);
        
        var user = await userService.GetUserByUsernameAsync(username);
        
        if (user == null)
        {
            logger.LogWarning("User not found: {Username}", username);
            return NotFound($"User '{username}' not found");
        }
        
        logger.LogDebug("User found: {Username}, Role: {Role}", username, user.Role);
        return Ok(user);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        logger.LogDebug("UserController.GetAllUsers() called");
        
        var users = await userService.GetAllUsersAsync();
        logger.LogDebug("Retrieved {Count} users from database", users.Count);
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] Models.AuthUser user)
    {
        logger.LogDebug("UserController.CreateUser() called with user: {@User}", user);
        
        if (string.IsNullOrWhiteSpace(user.Username))
        {
            logger.LogWarning("CreateUser called with invalid user data");
            return BadRequest("Invalid user data");
        }

        logger.LogDebug("Creating new user: {Username}, Role: {Role}", user.Username, user.Role);
        var result = await userService.CreateUserAsync(user);
        
        if (result)
        {
            logger.LogDebug("User created successfully: {Username}", user.Username);
            return CreatedAtAction(nameof(GetUser), new { username = user.Username }, user);
        }
        
        logger.LogWarning("User creation failed - user already exists: {Username}", user.Username);
        return Conflict($"User '{user.Username}' already exists");
    }
}
