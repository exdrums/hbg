using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

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
        // app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
    
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHub<ChatHub>("/chathub");
        });
        
        // Ensure file storage directory exists
        var fileStoragePath = app.Configuration["FileStorage:BasePath"] ?? Path.Combine(Path.GetTempPath(), "ChatFiles");
        if (!Directory.Exists(fileStoragePath))
        {
            Directory.CreateDirectory(fileStoragePath);
        }
    }
} 