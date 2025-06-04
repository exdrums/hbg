using API.Contacts.WebSocket;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace API.Contacts;

public static class Configure
{
    public static void ConfigureApp(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseCors("CorsPolicy");
        app.UseSwagger().UseSwaggerUI(c => // "/swagger"
        {
            c.SwaggerEndpoint($"/swagger/v1/swagger.json", "API.Contacts V1");
            c.OAuthClientId("client_contacts_swaggerui");
            c.OAuthUsePkce();
            c.OAuthAppName("Contacts Swagger UI");
        });

        // Security headers
        app.Use(async (context, next) =>
        {
            // context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            // context.Response.Headers.Add("X-Frame-Options", "DENY");
            // context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
            // context.Response.Headers.Add("Referrer-Policy", "no-referrer");

            // // Add CSP header for enhanced security
            // if (!app.Environment.IsDevelopment())
            // {
            //     context.Response.Headers.Add("Content-Security-Policy",
            //         "default-src 'self'; connect-src 'self' ws: wss:; script-src 'self' 'unsafe-inline'");
            // }

            await next();
        });


        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // Map SignalR hub endpoint
        app.MapHub<ChatHub>("/hubs/chat", options =>
        {
            // Configure hub-specific options
            options.Transports = HttpTransportType.WebSockets;
            

            // Add authorization policy
            options.ApplicationMaxBufferSize = 64 * 1024; // 64KB buffer
            options.TransportMaxBufferSize = 64 * 1024;
        });

        // Map health checks with detailed response
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteHealthCheckResponse
        });

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false, // Liveness check with no dependencies
            ResponseWriter = WriteHealthCheckResponse
        });

        // Comprehensive health check endpoint
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteHealthCheckResponse
        });

        // Enhanced minimal API endpoints for metadata
        app.MapGet("/", () => new
        {
            service = "Contacts Microservice",
            version = "1.0.0",
            features = new[]
            {
                "Real-time messaging with SignalR",
                "Multi-tab chat interface (Contacts, Support, Agent)",
                "Alert system for notifications",
                "DevExtreme DataSource integration",
                "Role-based access control"
            },
            endpoints = new[]
            {
                "/hubs/chat - SignalR chat hub",
                "/health/ready - Readiness probe",
                "/health/live - Liveness probe",
                "/health - Complete health status",
                "/swagger - API documentation"
            }
        }).AllowAnonymous();

        // Error handling endpoint
        app.MapGet("/error", () => Results.Problem("An error occurred"))
            .ExcludeFromDescription();

        // Ensure file storage directory exists
        // var fileStoragePath = app.Configuration["FileStorage:BasePath"] ?? Path.Combine(Path.GetTempPath(), "ChatFiles");
        // if (!Directory.Exists(fileStoragePath))
        // {
        //     Directory.CreateDirectory(fileStoragePath);
        // }
    }

    private static Task WriteHealthCheckResponse(HttpContext context, HealthReport result)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            status = result.Status.ToString(),
            checks = result.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                tags = e.Value.Tags
            }),
            totalDuration = result.TotalDuration.TotalMilliseconds,
            timestamp = DateTime.UtcNow
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
