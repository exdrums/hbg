using API.Contacts.Infrastructure.SignalR;
using System.IO;

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

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // Map SignalR hub endpoint
        app.MapHub<ChatHub>("/chathub");

        // Ensure file storage directory exists
        var fileStoragePath = app.Configuration["FileStorage:BasePath"] ?? Path.Combine(Path.GetTempPath(), "ChatFiles");
        if (!Directory.Exists(fileStoragePath))
        {
            Directory.CreateDirectory(fileStoragePath);
        }
    }
}
