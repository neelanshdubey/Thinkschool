using OrderApi.Services;
using OrderApi.Strategies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<OrderService>();

builder.Services.AddScoped<IOrderRule, NegativeQuantityRule>();
builder.Services.AddScoped<IOrderRule, ZeroQuantityRule>();
builder.Services.AddScoped<IOrderRule, MinimumOrderTotalRule>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program
{
}