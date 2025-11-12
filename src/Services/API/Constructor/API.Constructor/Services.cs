using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Common;
using Common.Exceptions;
using Common.WebSocket;
using API.Constructor.Data;
using API.Constructor.Services;
using API.Constructor.Mapping;

namespace API.Constructor
{
    public static class ConfigureServices
    {
        public static AppSettings RegisterServices(this IServiceCollection services, IConfiguration config)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            var appSettings = services.ConfigureAppSettings(config);
            RegisterIdentity(services, appSettings);
            RegisterSwagger(services, appSettings);
            RegisterCors(services, appSettings);

            services.AddAutoMapper(typeof(MappingProfile));

            // to access HttpContext from all services
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Database
            services.AddDbContext<ConstructorDbContext>(options =>
            {
                options.UseNpgsql(appSettings.HBGCONSTRUCTORDB);
            });

            // Services
            services.AddScoped<AuthService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IConfigurationService, ConfigurationService>();
            services.AddScoped<IGeminiImageService, GeminiImageService>();
            services.AddScoped<IFilesServiceClient, FilesServiceClient>();

            // HttpClient for Files Service
            services.AddHttpClient("FilesService", client =>
            {
                client.BaseAddress = new Uri(appSettings.HBGFILES);
                client.Timeout = TimeSpan.FromSeconds(60);
            });

            // SignalR
            services.AddSingleton<IUserIdProvider, UserIdProvider>();
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            }).AddJsonProtocol();

            services.AddControllers();

            return appSettings;
        }

        private static void RegisterIdentity(IServiceCollection services, AppSettings appSettings)
        {
            services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
            {
                options.Authority = appSettings.HBGIDENTITY;
                options.RequireHttpsMetadata = false;
                options.Audience = appSettings.AUDIENCE;

                options.Events = new JwtBearerEvents()
                {
                    // Used to authenticate WS connection
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"].FirstOrDefault();
                        if ((!string.IsNullOrEmpty(accessToken)) &&
                            context.HttpContext.Request.Path.StartsWithSegments("/hubs/constructor"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context => throw (context.Exception ?? new AuthenticationFailedException("Unauthorized")),
                    OnChallenge = context => throw new AccessDeniedException("Forbidden. Token challenge failed."),
                    OnForbidden = context => throw new AccessDeniedException("Forbidden.")
                };
            });
        }

        private static void RegisterSwagger(IServiceCollection services, AppSettings appSettings)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Constructor HTTP API",
                    Version = "v1",
                    Description = "The Jewelry Constructor Service HTTP API"
                });

                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{appSettings.HBGIDENTITY}/connect/authorize"),
                            TokenUrl = new Uri($"{appSettings.HBGIDENTITY}/connect/token"),
                            Scopes = new System.Collections.Generic.Dictionary<string, string>
                            {
                                { "api_constructor", "Constructor API" }
                            }
                        }
                    }
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            }
                        },
                        new[] { "api_constructor" }
                    }
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
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });
        }
    }
}
