using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using API.Files.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using API.Common;

namespace API.Files;

public static class ConfigureServices {
    public static void RegisterServices(this IServiceCollection services, IConfiguration config)
    {            
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        var appSettings = services.ConfigureAppSettings(config);
        RegisterIdentity(services, appSettings);
        RegisterSwagger(services, appSettings);
        RegisterCors(services, appSettings);
        // to access HttpContext from all services
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddTransient<AuthService>();
        services.AddTransient<FilesRepo>();

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
                Title = "Files HTTP API",
                Version = "v1",
                Description = "The Files Service HTTP API"
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
                            { "api_files", "Files API" }
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
                // .AllowAnyOrigin()
                // .SetIsOriginAllowed((host) => true)
                .WithOrigins(appSettings.HBGSPA, appSettings.HBGSPADEV)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
        });
    }
}