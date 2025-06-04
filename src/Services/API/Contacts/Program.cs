using Serilog;
using API.Contacts;
using API.Contacts.Data;
using Microsoft.EntityFrameworkCore;
using API.Common;
using Microsoft.Extensions.Options;

var options = new WebApplicationOptions()
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
};

var builder = WebApplication.CreateBuilder(options);

// Add configuration sources
builder.Configuration
    // .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    // .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

if (builder.Environment.IsProduction())
{
    // Add structured logging for production (e.g., Serilog, Application Insights)
    // builder.Logging.AddApplicationInsights();
}

// Add logging
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Host.UseSerilog((builderContext, config) => config.ReadFrom.Configuration(builderContext.Configuration, "Serilog"));

builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

app.ConfigureApp();

// Run database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    var test = scope.ServiceProvider.GetService<IOptionsSnapshot<AppSettings>>();
    if (app.Environment.IsDevelopment())
    {
        // In development, recreate database
        context.Database.EnsureDeleted();
        context.Database.Migrate();
        context.Database.EnsureCreated();
    }
    else
    {
        // In production, run migrations
        context.Database.Migrate();
    }
}

app.Run();
