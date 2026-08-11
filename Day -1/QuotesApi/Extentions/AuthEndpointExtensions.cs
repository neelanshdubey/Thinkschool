using Microsoft.EntityFrameworkCore;
using QuotesApi.Data;
using QuotesApi.Models;
using QuotesApi.Services;

namespace QuotesApi.Extensions;

public static class AuthEndpointExtensions
{
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/login", async (
            LoginRequest request,
            AppDbContext dbContext,
            ITokenService tokenService,
            IRefreshTokenService refreshTokenService) =>
        {
            var user = await dbContext.Users
                .SingleOrDefaultAsync(u => u.Email == request.Email);

            if (user is null || !BCryptNet.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Results.Unauthorized();
            }

            var accessToken = tokenService.CreateAccessToken(user);
            var refreshToken = await refreshTokenService.CreateRefreshTokenAsync(user);

            return Results.Ok(new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        });

        app.MapPost("/api/auth/refresh", async (
            RefreshRequest request,
            IRefreshTokenService refreshTokenService) =>
        {
            var response = await refreshTokenService.RefreshAsync(request.RefreshToken);
            return response is null ? Results.Unauthorized() : Results.Ok(response);
        });

        app.MapPost("/api/auth/logout", async (
            LogoutRequest request,
            IRefreshTokenService refreshTokenService) =>
        {
            var revoked = await refreshTokenService.RevokeAsync(request.RefreshToken);
            return revoked ? Results.NoContent() : Results.Unauthorized();
        });

        return app;
    }
}
