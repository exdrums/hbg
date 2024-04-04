using Microsoft.AspNetCore.Builder;
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
    }

}