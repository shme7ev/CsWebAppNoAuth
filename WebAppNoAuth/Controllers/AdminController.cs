using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAppNoAuth.Models;
using WebAppNoAuth.Services;

namespace WebAppNoAuth.Controllers;

[Authorize] // This attribute requires JWT authentication for all actions in this controller
public class AdminController : Controller
{
    private readonly IProductService _productService;
    private readonly IProductServiceEF _productServiceEF;
    private readonly IUserService _userService;

    public AdminController(IProductService productService, IProductServiceEF productServiceEF, IUserService userService)
    {
        _productService = productService;
        _productServiceEF = productServiceEF;
        _userService = userService;
    }

    private async Task<string> GetUserRoleAsync()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return "Unknown";

        var user = await _userService.GetUserByUsernameAsync(username);
        return user?.Role ?? "Unknown";
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new HomeViewModel();
        
        viewModel.RawSqlProducts = await _productService.GetAllProductsAsync();
        viewModel.RawSqlCount = viewModel.RawSqlProducts.Count;
        
        viewModel.EntityFrameworkProducts = await _productServiceEF.GetAllProductsAsync();
        viewModel.EntityFrameworkCount = viewModel.EntityFrameworkProducts.Count;
        
        ViewBag.Message = "Welcome to the Admin Dashboard! This section is protected and requires JWT authentication.";
        ViewBag.Username = User.Identity?.Name ?? "Authenticated User";
        ViewBag.UserRole = await GetUserRoleAsync();
        
        return View(viewModel);
    }

    // Method accessible only to users with Admin role
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UserManager()
    {
        var users = await _userService.GetAllUsersAsync();
        ViewBag.Message = "User Management - Admin Only";
        ViewBag.Username = User.Identity?.Name ?? "Authenticated User";
        ViewBag.UserRole = await GetUserRoleAsync();
        
        return View("UserManager", users);
    }

    // Method accessible to Admin and Manager roles
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Reports()
    {
        var users = await _userService.GetAllUsersAsync();
        var userStats = new
        {
            TotalUsers = users.Count,
            AdminUsers = users.Count(u => u.Role == "Admin"),
            ManagerUsers = users.Count(u => u.Role == "Manager"),
            RegularUsers = users.Count(u => u.Role == "User" || u.Role == "Developer")
        };
        
        ViewBag.Message = "Reports Dashboard - Admin and Managers Only";
        ViewBag.Username = User.Identity?.Name ?? "Authenticated User";
        ViewBag.UserRole = await GetUserRoleAsync();
        ViewBag.Stats = userStats;
        
        return View("Reports");
    }

    // Method accessible to all authenticated users
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Index");
        }
        
        var user = await _userService.GetUserByUsernameAsync(username);
        ViewBag.Message = "User Profile";
        ViewBag.Username = username;
        ViewBag.UserRole = await GetUserRoleAsync();
        
        return View("Profile", user);
    }

    // Method for testing different role access levels
    [HttpGet]
    [AllowAnonymous] // This allows access without authentication for testing purposes
    public IActionResult Login()
    {
        return View();
    }

    // [HttpPost]
    // [AllowAnonymous]
    // public async Task<IActionResult> GenerateToken([FromForm] string username)
    // {
    //     if (string.IsNullOrWhiteSpace(username))
    //     {
    //         ViewBag.Error = "Username is required";
    //         return View("Login");
    //     }
    //
    //     var user = await _userService.GetUserByUsernameAsync(username.Trim());
    //     if (user == null)
    //     {
    //         ViewBag.Error = "User not found";
    //         return View("Login");
    //     }
    //
    //     var tokenService = HttpContext.RequestServices.GetRequiredService<IJwtTokenService>();
    //     var token = tokenService.GenerateToken(user.Username);
    //
    //     ViewBag.Username = user.Username;
    //     ViewBag.Role = user.Role;
    //     ViewBag.Token = token;
    //     ViewBag.Message = "JWT token is ready for use!";
    //
    //     return View("Login");
    // }
}
