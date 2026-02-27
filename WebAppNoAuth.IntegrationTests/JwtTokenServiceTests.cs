using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using WebAppNoAuth.Services;

namespace WebAppNoAuth.IntegrationTests;

public class JwtTokenServiceTests(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly IJwtTokenService _tokenService = factory.Services.CreateScope().ServiceProvider.GetRequiredService<IJwtTokenService>();

    [Theory]
    [InlineData("user1")]
    [InlineData("admin@test.com")]
    [InlineData("user_with_underscores")]
    public void GenerateToken_Creates_Valid_Token_For_Various_Usernames(string username)
    {
        var token = _tokenService.GenerateToken(username);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(token));
        
        var jwtToken = handler.ReadJwtToken(token);
        Assert.Equal("WebAppNoAuth", jwtToken.Issuer);
        Assert.Equal("WebAppNoAuthUsers", jwtToken.Audiences.First());
        
        var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
        Assert.NotNull(nameClaim);
        Assert.Equal(username, nameClaim.Value);
        
        var nameIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        Assert.NotNull(nameIdClaim);
        Assert.Equal(username, nameIdClaim.Value);
        
        var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
        Assert.NotNull(jtiClaim);
        Assert.NotEmpty(jtiClaim.Value);
    }

    [Theory]
    [InlineData("user1")]
    public void Generated_Tokens_Are_Unique(string username)
    {
        var token1 = _tokenService.GenerateToken(username);
        var token2 = _tokenService.GenerateToken(username);

        Assert.NotEqual(token1, token2);
        
        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(token1));
        Assert.True(handler.CanReadToken(token2));
    }

    [Fact]
    public void Tokens_Contain_Expected_Expiration_Time()
    {
        var token = _tokenService.GenerateToken("expiringuser");

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);
        Assert.NotNull(expClaim);
        
        var expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value));
        var now = DateTimeOffset.UtcNow;
        
        Assert.True(expirationTime > now);
        Assert.True(expirationTime <= now.AddMinutes(61));
    }

    [Theory]
    [InlineData("validationtestuser")]
    public void Tokens_Can_Be_Validated_By_Same_Key(string username )
    {
        var token = _tokenService.GenerateToken(username);

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

        var claimsPrincipal = handler.ValidateToken(token, validationParameters, out var validatedToken);

        Assert.NotNull(claimsPrincipal);
        Assert.NotNull(validatedToken);
        Assert.Equal(username, claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value);
    }

    [Fact]
    public void Service_Handles_Multiple_Concurrent_Token_Generations()
    {
        var usernames = Enumerable.Range(1, 10).Select(i => $"concurrentuser{i}").ToArray();

        var tokens = usernames.AsParallel().Select(username => _tokenService.GenerateToken(username)).ToArray();

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
        var token = _tokenService.GenerateToken("sizetestuser");

        Assert.True(token.Length > 50);
        Assert.True(token.Length < 1000);
    }
}
