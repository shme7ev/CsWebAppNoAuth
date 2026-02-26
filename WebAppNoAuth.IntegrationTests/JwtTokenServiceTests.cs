using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using WebAppNoAuth.Services;

namespace WebAppNoAuth.IntegrationTests;

public class JwtTokenServiceTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public JwtTokenServiceTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Token_Service_Can_Be_Resolved_From_DI()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();

        // Act
        var tokenService = scope.ServiceProvider.GetService<IJwtTokenService>();

        // Assert
        Assert.NotNull(tokenService);
        Assert.IsType<JwtTokenService>(tokenService);
    }

    [Theory]
    [InlineData("user1")]
    [InlineData("admin@test.com")]
    [InlineData("user_with_underscores")]
    [InlineData("用户")] // Chinese characters
    public void GenerateToken_Creates_Valid_Token_For_Various_Usernames(string username)
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

        // Act
        var token = tokenService.GenerateToken(username);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        // Validate token structure
        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(token));
        
        var jwtToken = handler.ReadJwtToken(token);
        Assert.Equal("WebAppNoAuth", jwtToken.Issuer);
        Assert.Equal("WebAppNoAuthUsers", jwtToken.Audiences.First());
        
        // Check claims
        var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
        Assert.NotNull(nameClaim);
        Assert.Equal(username, nameClaim.Value);
        
        var nameIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        Assert.NotNull(nameIdClaim);
        Assert.Equal(username, nameIdClaim.Value);
        
        // Check JTI (JWT ID)
        var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
        Assert.NotNull(jtiClaim);
        Assert.NotEmpty(jtiClaim.Value);
    }

    [Fact]
    public void Generated_Tokens_Are_Unique()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var username = "testuser";

        // Act
        var token1 = tokenService.GenerateToken(username);
        var token2 = tokenService.GenerateToken(username);

        // Assert
        Assert.NotEqual(token1, token2);
        
        // Both should be valid tokens though
        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(token1));
        Assert.True(handler.CanReadToken(token2));
    }

    [Fact]
    public void Tokens_Contain_Expected_Expiration_Time()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

        // Act
        var token = tokenService.GenerateToken("expiringuser");

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);
        Assert.NotNull(expClaim);
        
        var expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value));
        var now = DateTimeOffset.UtcNow;
        
        // Token should expire in the future (within 60 minutes as configured)
        Assert.True(expirationTime > now);
        Assert.True(expirationTime <= now.AddMinutes(61)); // Allow small buffer
    }

    [Fact]
    public void Tokens_Can_Be_Validated_By_Same_Key()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var username = "validationtestuser";
        var token = tokenService.GenerateToken(username);

        // Act & Assert
        var handler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "WebAppNoAuth",
            ValidAudience = "WebAppNoAuthUsers",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ThisIsASecretKeyForJWTAuthentication12345!"))
        };

        SecurityToken validatedToken;
        var claimsPrincipal = handler.ValidateToken(token, validationParameters, out validatedToken);

        // Assert
        Assert.NotNull(claimsPrincipal);
        Assert.NotNull(validatedToken);
        Assert.Equal(username, claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value);
    }

    [Fact]
    public void Service_Handles_Multiple_Concurrent_Token_Generations()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var usernames = Enumerable.Range(1, 10).Select(i => $"concurrentuser{i}").ToArray();

        // Act
        var tokens = usernames.AsParallel().Select(username => tokenService.GenerateToken(username)).ToArray();

        // Assert
        Assert.Equal(usernames.Length, tokens.Length);
        foreach (var token in tokens)
        {
            Assert.NotNull(token);
            Assert.NotEmpty(token);
            
            var handler = new JwtSecurityTokenHandler();
            Assert.True(handler.CanReadToken(token));
        }
    }

    [Fact]
    public void Token_Length_Is_Reasonable()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

        // Act
        var token = tokenService.GenerateToken("sizetestuser");

        // Assert
        // JWT tokens should be reasonably sized (not too short, not excessively long)
        Assert.True(token.Length > 50); // Minimum reasonable size
        Assert.True(token.Length < 1000); // Maximum reasonable size
    }
}