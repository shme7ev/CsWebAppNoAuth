using Microsoft.AspNetCore.Mvc;
using WebAppNoAuth.Services;
using Microsoft.Extensions.Logging;

namespace WebAppNoAuth.Controllers;

public class TokenRequest
{
    public string? Username { get; set; }
}

public class LoginController(IJwtTokenService tokenService, ILogger<LoginController> logger)
    : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        logger.LogDebug("LoginController.Login() GET called");
        logger.LogDebug("Login page accessed");
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username)
    {
        logger.LogDebug("LoginController.Login() POST called with username: {Username}", username);
        
        if (string.IsNullOrWhiteSpace(username))
        {
            logger.LogWarning("Login attempt with empty username");
            ViewBag.Error = "Username is required";
            return View();
        }

        logger.LogDebug("Generating JWT token for user: {Username}", username);
        var token = tokenService.GenerateToken(username);
        ViewBag.Token = token;
        ViewBag.Username = username;
        ViewBag.Success = "Login successful! Your JWT token is ready.";
        ViewBag.Message = "JWT token is ready for use! It is added to your session";

        HttpContext.Session.SetString("Token", token);
        logger.LogDebug("JWT token stored in session for user: {Username}", username);

        return View("Login");
    }

    [HttpGet]
    [Route("api/login/token/{username}")]
    public IActionResult GetToken(string username)
    {
        logger.LogDebug("LoginController.GetToken() called with username: {Username}", username);
        
        if (string.IsNullOrWhiteSpace(username))
        {
            logger.LogWarning("GetToken called with empty username");
            return BadRequest(new { error = "Username is required" });
        }

        logger.LogDebug("Generating API token for user: {Username}", username);
        var token = tokenService.GenerateToken(username);
        logger.LogDebug("Generated token for user: {Username}", username);
        return Ok(new { token, username });
    }

    [HttpPost]
    [Route("api/login/token")]
    public IActionResult GetTokenPost([FromBody] TokenRequest request)
    {
        if (request.Username == null || string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new { error = "Username is required" });
        }

        var token = tokenService.GenerateToken(request.Username);
        return Ok(new { token, username = request.Username });
    }
}
