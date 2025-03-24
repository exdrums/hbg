using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using API.Common;
using API.Contacts.Services;
using API.Contacts.Data.Repositories;
using API.Contacts.Services.Interfaces;
using API.Contacts.Data;
using Microsoft.EntityFrameworkCore;

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

        // Register your services here
        // services.AddTransient<YourService>();

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
        // Add PostgreSQL database
        services.AddDbContext<ChatDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null)
            )
        );

        // Register configuration
        services.Configure<AiAssistantOptions>(configuration.GetSection("AiAssistant"));

        // Register domain services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<ITypingService, TypingService>();
        services.AddScoped<IAiAssistantService, AzureOpenAIAssistantService>();

        // Register application services
        services.AddScoped<IChatApplicationService, ChatApplicationService>();

        // Register infrastructure services
        services.AddHttpClient<IAiAssistantService, AzureOpenAIAssistantService>();
        services.AddSingleton<IConnectionManager, InMemoryConnectionManager>();
        services.AddScoped<IRealtimeNotificationService, SignalRNotificationService>();
        services.AddScoped<IRateLimitingService, DistributedCacheRateLimitingService>();
        services.AddScoped<IOidcAuthenticationService, OidcAuthenticationService>();
        services.AddScoped<IReadReceiptService, ReadReceiptService>();
        services.AddScoped<IFileMessageHandler, FileMessageHandler>();

        // Register SignalR
        services.AddSignalR();
    }
} 