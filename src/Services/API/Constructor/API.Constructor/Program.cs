using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using API.Constructor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using API.Constructor.Data;

var options = new WebApplicationOptions()
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
};

var builder = WebApplication.CreateBuilder(options);

builder.Configuration.AddEnvironmentVariables();

builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Host.UseSerilog((builderContext, config) => config.ReadFrom.Configuration(builderContext.Configuration, "Serilog"));

var appSettings = builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

// Database migration
using (var scope = app.Services.CreateScope())
{
    using (var context = scope.ServiceProvider.GetRequiredService<ConstructorDbContext>())
    {
        context.Database.Migrate();
    }
}

app.ConfigureApp();

app.Run();
