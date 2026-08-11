using CollectionsApi.Data;
using CollectionsApi.Repositories;
using CollectionsApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=collections.db"));

builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();
builder.Services.AddScoped<ICollectionService, CollectionService>();

var app = builder.Build();

app.MapGet("/api/collections", async (
    ICollectionService service,
    CancellationToken cancellationToken) =>
{
    var collections = await service.GetAllAsync(
        cancellationToken);

    return Results.Ok(collections);
});

app.Run();

public partial class Program
{
}

