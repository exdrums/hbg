using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

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
            c.SwaggerEndpoint($"/swagger/v1/swagger.json", "API.Files V1");
            c.OAuthClientId("client_files_swaggerui");
            c.OAuthUsePkce();
            c.OAuthAppName("Files Swagger UI");
        });
        // app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        // Map health checks with detailed response
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = _ => true,
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