using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using QuotesApi.Models;
using QuotesApi.Services;

namespace Quotes.Tests.Unit;

public class TokenServiceTests
{
    private static IConfiguration BuildConfiguration(int expiresInSeconds = 3600)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience",
            ["Jwt:Key"] = "this-is-a-sufficiently-long-test-signing-key-1234567890",
            ["Jwt:ExpiresIn"] = expiresInSeconds.ToString()
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    [Fact]
    public void CreateAccessToken_ValidUser_ReturnsTokenWithExpectedClaims()
    {
        // Arrange
        var configuration = BuildConfiguration();
        var tokenService = new TokenService(configuration);
        var user = new User { Id = 7, Email = "reader@example.com", PasswordHash = "hash" };

        // Act
        var token = tokenService.CreateAccessToken(user);

        // Assert
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Issuer.Should().Be("test-issuer");
        jwt.Audiences.Should().Contain("test-audience");
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "7");
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "reader@example.com");
        jwt.Claims.Should().Contain(c => c.Type == "scope" && c.Value == "quotes.write");
    }

    [Fact]
    public void CreateAccessToken_ValidUser_SetsExpiryBasedOnConfiguredExpiresIn()
    {
        // Arrange
        var configuration = BuildConfiguration(expiresInSeconds: 120);
        var tokenService = new TokenService(configuration);
        var user = new User { Id = 1, Email = "reader@example.com", PasswordHash = "hash" };
        var before = DateTime.UtcNow;

        // Act
        var token = tokenService.CreateAccessToken(user);

        // Assert
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var expectedExpiry = before.AddSeconds(120);
        jwt.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CreateAccessToken_TwoDifferentUsers_ProducesDifferentTokens()
    {
        // Arrange
        var configuration = BuildConfiguration();
        var tokenService = new TokenService(configuration);
        var userA = new User { Id = 1, Email = "a@example.com", PasswordHash = "hash" };
        var userB = new User { Id = 2, Email = "b@example.com", PasswordHash = "hash" };

        // Act
        var tokenA = tokenService.CreateAccessToken(userA);
        var tokenB = tokenService.CreateAccessToken(userB);

        // Assert
        tokenA.Should().NotBe(tokenB);
    }

    [Fact]
    public void CreateRefreshToken_ReturnsNonEmptyBase64String()
    {
        // Arrange
        var configuration = BuildConfiguration();
        var tokenService = new TokenService(configuration);

        // Act
        var refreshToken = tokenService.CreateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrWhiteSpace();
        var act = () => Convert.FromBase64String(refreshToken);
        act.Should().NotThrow();
    }

    [Fact]
    public void CreateRefreshToken_CalledTwice_ReturnsDifferentValues()
    {
        // Arrange
        var configuration = BuildConfiguration();
        var tokenService = new TokenService(configuration);

        // Act
        var first = tokenService.CreateRefreshToken();
        var second = tokenService.CreateRefreshToken();

        // Assert
        first.Should().NotBe(second);
    }

    [Fact]
    public void HashRefreshToken_SameInput_ReturnsSameHash()
    {
        // Arrange
        var configuration = BuildConfiguration();
        var tokenService = new TokenService(configuration);
        var rawToken = "a-fixed-raw-refresh-token-value";

        // Act
        var hash1 = tokenService.HashRefreshToken(rawToken);
        var hash2 = tokenService.HashRefreshToken(rawToken);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void HashRefreshToken_DifferentInputs_ReturnsDifferentHashes()
    {
        // Arrange
        var configuration = BuildConfiguration();
        var tokenService = new TokenService(configuration);

        // Act
        var hash1 = tokenService.HashRefreshToken("raw-token-one");
        var hash2 = tokenService.HashRefreshToken("raw-token-two");

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void RefreshTokenValidityInDays_ReturnsSeven()
    {
        // Arrange
        var configuration = BuildConfiguration();
        var tokenService = new TokenService(configuration);

        // Act
        var validityInDays = tokenService.RefreshTokenValidityInDays;

        // Assert
        validityInDays.Should().Be(7);
    }
}
