using Microsoft.AspNetCore.Mvc;
using WebAppNoAuth.Services;

namespace WebAppNoAuth.Controllers;

public class TokenRequest
{
    public string? Username { get; set; }
}

public class LoginController : Controller
{
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

        var tokenService = HttpContext.RequestServices.GetService<IJwtTokenService>();
        if (tokenService == null)
        {
            ViewBag.Error = "Token service not available";
            return View();
        }

        var token = tokenService.GenerateToken(username);
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

        var tokenService = HttpContext.RequestServices.GetService<IJwtTokenService>();
        if (tokenService == null)
        {
            return StatusCode(500, new { error = "Token service not available" });
        }

        var token = tokenService.GenerateToken(username);
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

        var tokenService = HttpContext.RequestServices.GetService<IJwtTokenService>();
        if (tokenService == null)
        {
            return StatusCode(500, new { error = "Token service not available" });
        }

        var token = tokenService.GenerateToken(request.Username);
        return Ok(new { token = token, username = request.Username });
    }
}