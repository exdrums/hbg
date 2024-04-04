using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using API.Projects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var options = new WebApplicationOptions()
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
};

var builder = WebApplication.CreateBuilder(options);

builder.Configuration.AddEnvironmentVariables();

// check needed?
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Host.UseSerilog((builderContext, config) => config.ReadFrom.Configuration(builderContext.Configuration, "Serilog"));

var appSettings = builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

using(var scope = app.Services.CreateScope()) 
{
    using(var context = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>()) 
    {
        context.Database.Migrate();
        context.SeedProjectsDb(appSettings);
    }
}

app.ConfigureApp();

app.Run();
