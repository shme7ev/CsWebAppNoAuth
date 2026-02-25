using Microsoft.AspNetCore.Mvc;
using WebAppNoAuth.Services;

namespace WebAppNoAuth.Controllers;

public class TokenRequest
{
    public string? Username { get; set; }
}

public class LoginController : Controller
{
    private readonly IJwtTokenService _tokenService;
    
    public LoginController(IJwtTokenService tokenService)
    {
        _tokenService = tokenService;
    }
    
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            ViewBag.Error = "Username is required";
            return View();
        }

        var token = _tokenService.GenerateToken(username);
        ViewBag.Token = token;
        ViewBag.Username = username;
        ViewBag.Success = "Login successful! Your JWT token is ready.";
        
        return View();
    }

    [HttpGet]
    [Route("api/login/token/{username}")]
    public IActionResult GetToken(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest(new { error = "Username is required" });
        }

        var token = _tokenService.GenerateToken(username);
        return Ok(new { token = token, username = username });
    }

    [HttpPost]
    [Route("api/login/token")]
    public IActionResult GetTokenPost([FromBody] TokenRequest request)
    {
        if (request?.Username == null || string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new { error = "Username is required" });
        }

        var token = _tokenService.GenerateToken(request.Username);
        return Ok(new { token = token, username = request.Username });
    }
}