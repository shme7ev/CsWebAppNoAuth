using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WebAppNoAuth.Services;

namespace WebAppNoAuth.IntegrationTests;

public class AdminControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AdminControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Admin_Index_Returns_Success_With_Valid_Token()
    {
        // Arrange
        var token = GetValidToken("adminindexuser");
        var request = new HttpRequestMessage(HttpMethod.Get, "/Admin/Index");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Admin Dashboard", content);
        Assert.Contains("adminindexuser", content);
        Assert.Contains("Protected Content", content);
    }

    [Fact]
    public async Task Admin_Dashboard_Returns_Success_With_Valid_Token()
    {
        // Arrange
        var token = GetValidToken("admindashboarduser");
        var request = new HttpRequestMessage(HttpMethod.Get, "/Admin/Dashboard");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Advanced Admin Dashboard", content);
        Assert.Contains("admindashboarduser", content);
        Assert.Contains("Secure Administrative Zone", content);
    }

    [Fact]
    public async Task Admin_Login_Get_Returns_Login_Form()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/Admin/Login");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Admin Login", content);
        Assert.Contains("username", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Generate JWT Token", content);
    }

    [Theory]
    [InlineData("simpleuser")]
    [InlineData("complex.user@domain.com")]
    [InlineData("user_with_special_chars_123")]
    public async Task Admin_Login_Post_Generates_Valid_Token_For_Various_Usernames(string username)
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "username", username }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await _client.PostAsync("/Admin/Login", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains(username, responseContent);
        Assert.Contains("JWT token is ready", responseContent);
        Assert.Contains("<textarea", responseContent);
    }

    [Fact]
    public async Task Admin_GenerateToken_Get_Requires_Authentication()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/Admin/GenerateToken");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        // This endpoint requires authentication (no [AllowAnonymous])
    }

    [Fact]
    public async Task Admin_Controller_Redirects_Unauthenticated_Users()
    {
        // Arrange
        var endpoints = new[] { "/Admin", "/Admin/Index", "/Admin/Dashboard" };
        
        foreach (var endpoint in endpoints)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }

    [Fact]
    public async Task Admin_Navigation_Links_Work_Correctly()
    {
        // Arrange
        var token = GetValidToken("navigationuser");
        
        // Test navigation from Admin Index to Dashboard
        var indexRequest = new HttpRequestMessage(HttpMethod.Get, "/Admin");
        indexRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var indexResponse = await _client.SendAsync(indexRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, indexResponse.StatusCode);
        var indexContent = await indexResponse.Content.ReadAsStringAsync();
        Assert.Contains("/Admin/Dashboard", indexContent);
        Assert.Contains("/Admin/Login", indexContent);
        Assert.Contains("/", indexContent); // Link back to home
    }

    [Fact]
    public async Task Admin_Views_Display_User_Identity_Correctly()
    {
        // Arrange
        var username = "identitytestuser";
        var token = GetValidToken(username);
        var request = new HttpRequestMessage(HttpMethod.Get, "/Admin");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(username, content);
        Assert.Contains("Authenticated User:", content);
    }

    [Fact]
    public async Task Admin_Pages_Show_Appropriate_Security_Indicators()
    {
        // Arrange
        var token = GetValidToken("securityuser");
        var request = new HttpRequestMessage(HttpMethod.Get, "/Admin");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("🔒 Protected Content", content);
        Assert.Contains("✅ Authenticated", content);
        Assert.Contains("JWT Token Status", content);
    }

    [Fact]
    public async Task Admin_Login_Copy_Functionality_Html_Structure()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "username", "copytestuser" }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await _client.PostAsync("/Admin/Login", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        // Check for the copy functionality elements
        Assert.Contains("textarea", responseContent);
        Assert.Contains("readonly", responseContent);
        Assert.Contains("onclick=\"this.select()\"", responseContent);
        Assert.Contains("📋 Copy Token", responseContent);
    }

    [Fact]
    public async Task Admin_Pages_Have_Proper_ViewData_Titles()
    {
        // Arrange
        var token = GetValidToken("titleuser");
        
        var requests = new[]
        {
            new { Url = "/Admin", ExpectedTitle = "Admin Dashboard" },
            new { Url = "/Admin/Dashboard", ExpectedTitle = "Admin Advanced Dashboard" },
            new { Url = "/Admin/Login", ExpectedTitle = "Admin Login" }
        };

        foreach (var requestInfo in requests)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestInfo.Url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(requestInfo.ExpectedTitle, content);
        }
    }

    [Fact]
    public async Task Admin_Views_Contain_Expected_UI_Components()
    {
        // Arrange
        var token = GetValidToken("uicomponentuser");
        var request = new HttpRequestMessage(HttpMethod.Get, "/Admin");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        
        // Check for bootstrap components
        Assert.Contains("alert alert-success", content);
        Assert.Contains("card", content);
        Assert.Contains("btn btn-warning", content);  // Advanced Dashboard button
        Assert.Contains("btn btn-info", content);     // Generate New Token button
        Assert.Contains("btn btn-secondary", content); // Back to Public Home button
        Assert.Contains("table", content);
        Assert.Contains("badge", content);
    }

    private string GetValidToken(string username)
    {
        var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        return tokenService.GenerateToken(username);
    }
}
