using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAppNoAuth.Models;
using WebAppNoAuth.Services;
using Microsoft.Extensions.Logging;

namespace WebAppNoAuth.Controllers;

[Authorize] // This attribute requires JWT authentication for all actions in this controller
public class AdminController : Controller
{
    private readonly IProductService _productService;
    private readonly IProductServiceEF _productServiceEF;
    private readonly IUserService _userService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IProductService productService, IProductServiceEF productServiceEF, IUserService userService, ILogger<AdminController> logger)
    {
        _productService = productService;
        _productServiceEF = productServiceEF;
        _userService = userService;
        _logger = logger;
    }

    private async Task<string> GetUserRoleAsync()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username)) return "Unknown";

        var user = await _userService.GetUserByUsernameAsync(username);
        _logger.LogDebug("Retrieved role for user {Username}: {Role}", username, user?.Role ?? "Unknown");
        return user?.Role ?? "Unknown";
    }

    public async Task<IActionResult> Index()
    {
        _logger.LogDebug("AdminController.Index() called");
        var username = User.Identity?.Name ?? "Unknown";
        _logger.LogDebug("Admin dashboard accessed by user: {Username}", username);
        
        var viewModel = new HomeViewModel();

        _logger.LogDebug("Fetching products for admin dashboard");
        viewModel.RawSqlProducts = await _productService.GetAllProductsAsync();
        viewModel.RawSqlCount = viewModel.RawSqlProducts.Count;

        viewModel.EntityFrameworkProducts = await _productServiceEF.GetAllProductsAsync();
        viewModel.EntityFrameworkCount = viewModel.EntityFrameworkProducts.Count;

        ViewBag.Message = "Welcome to the Admin Dashboard! This section is protected and requires JWT authentication.";
        ViewBag.Username = username;
        ViewBag.UserRole = await GetUserRoleAsync();

        _logger.LogDebug("Admin dashboard loaded with {RawSqlCount} (SQL) and {EntityFrameworkCount} (EF) products for user: {Username}",
                             viewModel.RawSqlCount, viewModel.EntityFrameworkCount, username);
        return View(viewModel);
    }

    // Method accessible only to users with Admin role
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UserManager()
    {
        _logger.LogDebug("AdminController.UserManager() called");
        var username = User.Identity?.Name ?? "Unknown";
        _logger.LogDebug("User management accessed by admin user: {Username}", username);
        
        var users = await _userService.GetAllUsersAsync();
        ViewBag.Message = "User Management - Admin Only";
        ViewBag.Username = username;
        ViewBag.UserRole = await GetUserRoleAsync();

        _logger.LogDebug("Loaded {UserCount} users for management by admin: {Username}", users.Count, username);
        return View("UserManager", users);
    }

    // Method accessible to Admin and Manager roles
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Reports()
    {
        _logger.LogDebug("AdminController.Reports() called");
        var username = User.Identity?.Name ?? "Unknown";
        var userRole = await GetUserRoleAsync();
        _logger.LogDebug("Reports dashboard accessed by user: {Username}, Role: {UserRole}", username, userRole);
        
        var users = await _userService.GetAllUsersAsync();
        var userStats = new
        {
            TotalUsers = users.Count,
            AdminUsers = users.Count(u => u.Role == "Admin"),
            ManagerUsers = users.Count(u => u.Role == "Manager"),
            RegularUsers = users.Count(u => u.Role == "User" || u.Role == "Developer")
        };

        ViewBag.Message = "Reports Dashboard - Admin and Managers Only";
        ViewBag.Username = username;
        ViewBag.UserRole = userRole;
        ViewBag.Stats = userStats;

        _logger.LogDebug("Reports generated - Total: {TotalUsers}, Admins: {AdminUsers}, Managers: {ManagerUsers}, Regular: {RegularUsers}",
                             userStats.TotalUsers, userStats.AdminUsers, userStats.ManagerUsers, userStats.RegularUsers);
        return View("Reports");
    }

    // Method accessible to all authenticated users
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        _logger.LogDebug("AdminController.Profile() called");
        var username = User.Identity?.Name;
        
        if (string.IsNullOrEmpty(username))
        {
            _logger.LogWarning("Profile access attempted without username, redirecting to index");
            return RedirectToAction("Index");
        }

        _logger.LogDebug("Loading profile for user: {Username}", username);
        var user = await _userService.GetUserByUsernameAsync(username);
        ViewBag.Message = "User Profile";
        ViewBag.Username = username;
        ViewBag.UserRole = await GetUserRoleAsync();

        _logger.LogDebug("Profile loaded for user: {Username}, Role: {Role}", username, user?.Role ?? "Unknown");
        return View("Profile", user);
    }

    // Method for testing different role access levels
    [HttpGet]
    [AllowAnonymous] // This allows access without authentication for testing purposes
    public IActionResult Login()
    {
        _logger.LogDebug("AdminController.Login() called (AllowAnonymous)");
        _logger.LogDebug("Admin login page accessed");
        return View();
    }
}
