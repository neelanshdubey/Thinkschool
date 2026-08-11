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
            ITokenService tokenService) =>
        {
            var user = await dbContext.Users
                .SingleOrDefaultAsync(u => u.Email == request.Email);

            if (user is null || !BCryptNet.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Results.Unauthorized();
            }

            var accessToken = tokenService.CreateAccessToken(user);
            var refreshToken = tokenService.CreateRefreshToken();

            return Results.Ok(new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = int.Parse(app.Configuration["Jwt:ExpiresIn"] ?? "3600")
            });
        });

        return app;
    }
}
