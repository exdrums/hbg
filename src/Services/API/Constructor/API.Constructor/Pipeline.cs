using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using API.Constructor.WebSocket;

namespace API.Constructor
{
    public static class Configure
    {
        public static void ConfigureApp(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");

            app.UseSwagger().UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/v1/swagger.json", "API.Constructor V1");
                c.OAuthClientId("client_constructor_swaggerui");
                c.OAuthUsePkce();
                c.OAuthAppName("Constructor Swagger UI");
            });

            // app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSecHeaders();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                // Only WebSocket connection allowed for SignalR
                endpoints.MapHub<ConstructorHub>("/hubs/constructor", options => options.Transports = HttpTransportType.WebSockets);
            });

            // Map health checks with detailed response
            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready") || check.Name == "database" || check.Name == "gemini",
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

        /// <summary>
        /// Add custom security headers to the response
        /// </summary>
        public static IApplicationBuilder UseSecHeaders(this IApplicationBuilder app) => app.Use(async (context, next) =>
        {
            // disable <frame>, <iframe>, <embed> or <object> ability
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options
            context.Response.Headers.Add("X-Frame-Options", "deny");
            // Blocks a request if the request destination is of type style and the MIME type is not text/css, or of type script and the MIME type is not a JavaScript MIME type
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-XSS-Protection
            context.Response.Headers.Add("X-Xss-Protection", "1");
            await next();
        });
    }
}
