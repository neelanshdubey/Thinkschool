using Microsoft.AspNetCore.Authorization;
using QuotesApi.Authorization;
using QuotesApi.Extensions;
using QuotesApi.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfig) =>
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSingleton<IAuthorizationHandler, SameOwnerAuthorizationHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("can-edit-quotes", policy =>
        policy.RequireClaim("scope", "quotes.write"));

    options.AddPolicy("can-delete-own-quote", policy =>
        policy.Requirements.Add(new SameOwnerRequirement()));
});

var app = builder.Build();

app.Use((context, next) =>
{
    using (Serilog.Context.LogContext.PushProperty("TraceId", context.TraceIdentifier))
    {
        return next();
    }
});

app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.ApplyMigrations();
}

app.MapGet("/", () => "Quotes API is running!");

app.MapAuthEndpoints();
app.MapQuoteEndpoints();

app.Run();