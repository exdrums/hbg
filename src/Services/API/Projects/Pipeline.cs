using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Hosting;
using Projects.WebSocket;

namespace API.Projects;

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
            c.SwaggerEndpoint($"/swagger/v1/swagger.json", "API.Projects V1");
            c.OAuthClientId("client_projects_swaggerui");
            c.OAuthUsePkce();
            c.OAuthAppName("Projects Swagger UI");
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
            endpoints.MapHub<ProjectsHub>("/hub/proj", options => options.Transports = HttpTransportType.WebSockets);
        });
    }

    /// <summary>
	/// Add custom security headers to the response, 
	/// </summary>
	public static IApplicationBuilder UseSecHeaders(this IApplicationBuilder app) => app.Use(async (context, next) =>
	{
		// disable <frame>, <iframe>, <embed> or <object> ability
		//// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options
		context.Response.Headers.Add("X-Frame-Options", "deny");
		////Blocks a request if the request destination is of type style and the MIME type is not text/css, or of type script and the MIME type is not a JavaScript MIME type
		//// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
		context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
		//// already not really necessary, but here
		//// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-XSS-Protection
		context.Response.Headers.Add("X-Xss-Protection", "1");
		await next();
	});
}