using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using API.Files;

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

builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

app.ConfigureApp();

app.Run();
