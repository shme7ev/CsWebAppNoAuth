using Microsoft.AspNetCore.Mvc;
using WebAppNoAuth.Services;

namespace WebAppNoAuth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> GetUser(string username)
    {
        var user = await _userService.GetUserByUsernameAsync(username);
        
        if (user == null)
        {
            return NotFound($"User '{username}' not found");
        }
        
        return Ok(user);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] Models.AuthUser user)
    {
        if (user == null || string.IsNullOrWhiteSpace(user.Username))
        {
            return BadRequest("Invalid user data");
        }

        var result = await _userService.CreateUserAsync(user);
        
        if (result)
        {
            return CreatedAtAction(nameof(GetUser), new { username = user.Username }, user);
        }
        
        return Conflict($"User '{user.Username}' already exists");
    }
}