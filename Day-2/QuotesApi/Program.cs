using QuotesApi.Extensions;
using QuotesApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure();

var app = builder.Build();

// 👇 YAHAN ADD KAR
app.UseMiddleware<ExceptionMiddleware>();

app.ApplyMigrations();

app.MapGet("/", () => "Quotes API is running!");

app.MapQuoteEndpoints();

app.Run();