using Microsoft.AspNetCore.Authorization;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("QuotesApi"))
    .WithTracing(t =>
    {
        t.AddAspNetCoreInstrumentation()
         .AddEntityFrameworkCoreInstrumentation()
         .AddHttpClientInstrumentation()
         .AddSource("QuotesApi");

        var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            // The OTLP exporter defaults to gRPC, which requires HTTP/2 cleartext support
            // when the endpoint is plain http:// (as it is for local collectors like Jaeger/Aspire).
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            t.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
        }
    });

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
    var traceId = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
    using (Serilog.Context.LogContext.PushProperty("TraceId", traceId))
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