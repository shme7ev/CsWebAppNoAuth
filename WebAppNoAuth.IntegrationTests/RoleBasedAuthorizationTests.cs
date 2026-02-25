using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WebAppNoAuth.Services;

namespace WebAppNoAuth.IntegrationTests;

public class RoleBasedAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public RoleBasedAuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Admin_User_Can_Access_UserManager_Endpoint()
    {
        // Arrange
        var token = GenerateValidToken("admin");
        var request = new HttpRequestMessage(HttpMethod.Get, "/Admin/UserManager");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("User Management - Admin Only", content);
        Assert.Contains("admin", content);
    }

    [Fact]
    public async Task Non_Admin_User_Cannot_Access_UserManager_Endpoint()
    {
        // Arrange
        var token = GenerateValidToken("user");
        var request = new HttpRequestMessage(HttpMethod.Get, "/Admin/UserManager");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_User_Can_Access_Reports_Endpoint()
    {
        // Arrange
        var token = GenerateValidToken("admin");
        var request = new HttpRequestMessage(HttpMethod.Get, "/Admin/Reports");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Reports Dashboard - Admin and Managers Only", content);
        Assert.Contains("admin", content);
    }

    [Fact]
    public async Task Manager_User_Can_Access_Reports_Endpoint()
    {
        // Arrange
        var token = GenerateValidToken("manager");
        var request = new HttpRequestMessage(HttpMethod.Get, "/Admin/Reports");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Reports Dashboard - Admin and Managers Only", content);
        Assert.Contains("manager", content);
    }

    [Fact]
    public async Task Regular_User_Cannot_Access_Reports_Endpoint()
    {
        // Arrange
        var token = GenerateValidToken("user");
        var request = new HttpRequestMessage(HttpMethod.Get, "/Admin/Reports");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Authenticated_User_Can_Access_Profile_Endpoint()
    {
        // Arrange
        var token = GenerateValidToken("user");
        var request = new HttpRequestMessage(HttpMethod.Get, "/Admin/Profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("User Profile", content);
        Assert.Contains("user", content);
    }

    [Fact]
    public async Task Unauthenticated_User_Cannot_Access_Profile_Endpoint()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/Admin/Profile");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Token_WorksWith_User_Lookup()
    {
        // Arrange
        var token = GenerateValidToken("admin");
        
        // Act & Assert
        // The token should be valid and user role should be looked up
        var request = new HttpRequestMessage(HttpMethod.Get, "/Admin");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("admin", content);
        Assert.Contains("Admin", content);
    }

    [Fact]
    public async Task Different_Roles_Get_Appropriate_Access_Level_Display()
    {
        // Test Admin role access display
        var adminResponse = await GetAdminDashboardResponse("admin");
        var adminContent = await adminResponse.Content.ReadAsStringAsync();
        Assert.Contains("Reports Dashboard", adminContent);
        Assert.Contains("User Management", adminContent);

        // Test Manager role access display
        var managerResponse = await GetAdminDashboardResponse("manager");
        var managerContent = await managerResponse.Content.ReadAsStringAsync();
        Assert.Contains("Reports Dashboard", managerContent);
        Assert.Contains("User Management", managerContent);

        // Test User role access display
        var userResponse = await GetAdminDashboardResponse("user");
        var userContent = await userResponse.Content.ReadAsStringAsync();
        Assert.Contains("Reports Dashboard", userContent);
        Assert.Contains("User Management", userContent);
    }

    [Fact]
    public async Task NonExistent_User_Cannot_Get_Valid_Token()
    {
        // Arrange - Try to get token for non-existent user
        var token = GenerateValidToken("nonexistentuser");
        var request = new HttpRequestMessage(HttpMethod.Get, "/Admin/UserManager");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        // This should still be forbidden because the user doesn't exist
        // and the authorization will fail
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private string GenerateValidToken(string username)
    {
        var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        return tokenService.GenerateToken(username);
    }

    private async Task<HttpResponseMessage> GetAdminDashboardResponse(string username)
    {
        var token = GenerateValidToken(username);
        var request = new HttpRequestMessage(HttpMethod.Get, "/Admin");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _client.SendAsync(request);
    }
}
