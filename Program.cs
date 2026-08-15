using OrderApi.Services;
using OrderApi.Strategies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<OrderService>();

builder.Services.AddScoped<IOrderRule, InvalidCustomerIdRule>();
builder.Services.AddScoped<IOrderRule, EmptyItemsRule>();
builder.Services.AddScoped<IOrderRule, NegativeQuantityRule>();
builder.Services.AddScoped<IOrderRule, InsufficientStockRule>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program
{
}