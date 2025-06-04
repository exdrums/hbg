using System.IdentityModel.Tokens.Jwt;
using Microsoft.OpenApi.Models;
using API.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using API.Contacts.Data;
using Microsoft.EntityFrameworkCore;
using API.Contacts.Services.Interfaces;
using API.Contacts.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.SignalR;
using Common.WebSocket;

namespace API.Contacts;

public static class ConfigureServices
{
    public static void RegisterServices(this IServiceCollection services, IConfiguration config)
    {
        // JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        var appSettings = services.ConfigureAppSettings(config);
        RegisterCors(services, appSettings);
        RegisterIdentity(services, appSettings);
        RegisterSwagger(services, appSettings);
        RegisterChatServices(services, appSettings);
        RegisterSignalR(services, appSettings);

        // to access HttpContext from all services
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        RegisterAdditional(services, appSettings);
    }

    private static void RegisterIdentity(IServiceCollection services, AppSettings appSettings)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Authority = appSettings.HBGIDENTITY;
            options.RequireHttpsMetadata = false;
            options.Audience = appSettings.AUDIENCE;

            // Configure for SignalR
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Read token from query string for SignalR
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };

            // Token validation parameters
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                // ValidateAudience = true,
                // ValidateLifetime = true,
                // ValidateIssuerSigningKey = true,
                // ClockSkew = TimeSpan.Zero // Strict token expiration
            };
            options.BackchannelHttpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
        });

        services.AddAuthorization(options =>
        {
            // Basic policies
            // options.AddPolicy("ChatUser", policy => policy.RequireAuthenticatedUser());
            // options.AddPolicy("ChatSupport", policy => policy.RequireClaim("role", "hbg-chat-support"));
            // options.AddPolicy("ChatAdmin", policy => policy.RequireClaim("role", "hbg-chat-admin"));

            // // Conversation type specific policies
            // options.AddPolicy("SupportConversation", policy =>
            //     policy.RequireAssertion(context =>
            //         context.User.HasClaim("role", "hbg-chat-support") ||
            //         context.User.Identity.IsAuthenticated));
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

    public static void RegisterChatServices(IServiceCollection services, AppSettings appSettings)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        // services.AddSingleton<IUserIdProvider, UserIdProvider>();
        // Add Entity Framework Core
        services.AddDbContext<ChatDbContext>(options =>
        {
            options.UseNpgsql(appSettings.HBGCONTACTSDB);
        });

        // Register services with appropriate lifetimes
        // Scoped services (per request/connection)
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IAlertService, AlertService>(); // New alert service

        // Singleton services (shared across all requests)
        services.AddSingleton<IUserConnectionService, UserConnectionService>();

        // Background services for maintenance and cleanup
        // services.AddHostedService<ConnectionCleanupService>();
        // services.AddHostedService<MessageRetentionService>();
        // services.AddHostedService<AlertCleanupService>();
    }

    private static void RegisterSignalR(IServiceCollection services, AppSettings appSettings)
    {
        // Add SignalR with enhanced configuration
        services.AddSignalR(options =>
        {
            // Configure SignalR options
            options.EnableDetailedErrors = true;
            // options.MaximumReceiveMessageSize = 102400; // 100KB
            // options.StreamBufferCapacity = 10;

            // // Configure timeouts
            // options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            // options.KeepAliveInterval = TimeSpan.FromSeconds(15);

            // // Additional configuration for chat functionality
            // options.MaximumParallelInvocationsPerClient = 1;
            // options.HandshakeTimeout = TimeSpan.FromSeconds(15);
        })
        .AddJsonProtocol(options =>
        {
            // // Configure JSON serialization for SignalR
            // options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            // options.PayloadSerializerOptions.PropertyNamingPolicy = null; // Keep property names as-is
            // options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // Serialize enums as strings
            // options.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
    }

    private static void RegisterAdditional(IServiceCollection services, AppSettings appSettings)
    {
        // Add health checks
        services.AddHealthChecks()
            .AddDbContextCheck<ChatDbContext>("database")
            .AddCheck("signalr", () => HealthCheckResult.Healthy("SignalR is ready"))
            .AddCheck("alert-service", () => HealthCheckResult.Healthy("Alert service is ready"));

        // Add controllers for any REST endpoints
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keep property names as-is
        });
        // Add API documentation with enhanced security definitions
        services.AddEndpointsApiExplorer();
    }
}
