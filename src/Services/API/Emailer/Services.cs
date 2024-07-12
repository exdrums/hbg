using System.IdentityModel.Tokens.Jwt;
using API.Common;
using API.Emailer.Database;
using API.Emailer.Services;
using Common.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace API.Emailer;

public static class ConfigureServices
{
    public static AppSettings RegisterServices(this IServiceCollection services, IConfiguration config)
    {            
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        var appSettings = services.ConfigureAppSettings(config);
        RegisterIdentity(services, appSettings);
        RegisterSwagger(services, appSettings);
        RegisterCors(services, appSettings);

        services.AddAutoMapper(typeof(AutoMapperProfile));

        // to access HttpContext from all services
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddDbContext<AppDbContext>(options => {
            options.UseNpgsql(appSettings.HBGEMAILERDB);
        });

        // services.AddScoped<EmailerService>();

        // services.AddSingleton<IUserIdProvider, UserIdProvider>();
        // services.AddSignalR(options =>
        // {
        //     options.EnableDetailedErrors = true;
        // }).AddJsonProtocol();

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
						context.HttpContext.Request.Path.StartsWithSegments("/hub"))
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
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Emailer HTTP API",
                Version = "v1",
                Description = "The Emailer Service HTTP API"
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
                            { "api_emailer", "Emailer API" }
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
