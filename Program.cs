using FinanceMCP.DB;
using FinanceMCP.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.AspNetCore;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

var apiKey = builder.Configuration["ApiKey"]
             ?? throw new InvalidOperationException("ApiKey is not configured");

var mongoConnection = builder.Configuration["MongoConnectionString"]
                      ?? throw new InvalidOperationException("MongoConnectionString is not configured");

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnection));
builder.Services.AddSingleton<FinanceDatabase>();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<Accounts>()
    .WithTools<Transactions>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/health"))
    {
        await next();
        return;
    }

    if (!context.Request.Headers.TryGetValue("X-Api-Key", out var key) || key != apiKey)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }

    await next();
});

app.MapMcp("/mcp");
app.MapGet("/health", () => "ok");

app.Run();