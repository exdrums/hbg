using API.Emailer;
using API.Emailer.Database;
using Microsoft.EntityFrameworkCore;
using Serilog;

var options = new WebApplicationOptions()
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
};

var builder = WebApplication.CreateBuilder(args);

// Add environmental variables
builder.Configuration.AddEnvironmentVariables();

// check needed?
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Host.UseSerilog((builderContext, config) => config.ReadFrom.Configuration(builderContext.Configuration, "Serilog"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var appSettings = builder.Services.RegisterServices(builder.Configuration);


var app = builder.Build();

using(var scope = app.Services.CreateScope()) 
{
    using(var context = scope.ServiceProvider.GetRequiredService<AppDbContext>()) 
    {
        context.Database.Migrate();
        // context.SeedProjectsDb(appSettings);
    }
}

app.ConfigureApp();

app.Run();
