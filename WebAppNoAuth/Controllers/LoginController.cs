using Microsoft.AspNetCore.Mvc;
using WebAppNoAuth.Services;

namespace WebAppNoAuth.Controllers;

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
}
