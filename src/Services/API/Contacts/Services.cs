using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using API.Common;
using API.Contacts.Infrastructure.DependencyInjection;
using API.Contacts.Infrastructure.SignalR;

namespace API.Contacts;

public static class ConfigureServices
{
    public static void RegisterServices(this IServiceCollection services, IConfiguration config)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        var appSettings = services.ConfigureAppSettings(config);
        RegisterIdentity(services, appSettings);
        RegisterSwagger(services, appSettings);
        RegisterCors(services, appSettings);
        RegisterChatServices(services, config);

        // to access HttpContext from all services
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.AddControllers();
    }

    private static void RegisterIdentity(IServiceCollection services, AppSettings appSettings)
    {
        services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
        {
            options.Authority = appSettings.HBGIDENTITY;
            options.RequireHttpsMetadata = false;
            options.Audience = appSettings.AUDIENCE;
        });
    }

    private static void RegisterSwagger(IServiceCollection services, AppSettings appSettings)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Contacts HTTP API",
                Version = "v1",
                Description = "The Contacts Service HTTP API"
            });

            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{appSettings.HBGIDENTITY}/connect/authorize"),
                        TokenUrl = new Uri($"{appSettings.HBGIDENTITY}/connect/token"),
                        Scopes = new Dictionary<string, string>()
                        {
                            { "api_contacts", "Contacts API" }
                        }
                    }
                },
                In = ParameterLocation.Header,
                Name = "Authorization"
            });
        });
    }

    private static void RegisterCors(IServiceCollection services, AppSettings appSettings)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder
                .WithOrigins(appSettings.HBGSPA, appSettings.HBGSPADEV)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
        });
    }

    public static void RegisterChatServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register all chat services from our refactored architecture
        services.AddChatServices();

        // Register AI assistant configuration
        services.Configure<AiAssistantOptions>(configuration.GetSection("AiAssistant"));

        // Register SignalR hub
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.MaximumReceiveMessageSize = 102400; // 100 KB
        });
    }
}

public class AiAssistantOptions
{
    public string Endpoint { get; set; }
    public string ApiKey { get; set; }
    public string DeploymentName { get; set; }
    public int MaxRequestsPerHour { get; set; } = 10;
    public int MaxTokens { get; set; } = 2000;
    public float Temperature { get; set; } = 0.7f;
    public string SystemPrompt { get; set; } = "You are a helpful assistant.";
}
