using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using API.Common;

namespace API.Identity.Admin.Spa
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var settings = services.ConfigureAppSettings(Configuration);
            this.ConfigureHealthChecks(services, settings);

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .SetIsOriginAllowed((host) => true)
                    .WithOrigins(settings.HBGIDENTITYADMINSPADEV)
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
            services.AddControllersWithViews();
            // In production Angular App will be served from this directory
            services.AddSpaStaticFiles(config => config.RootPath = "Client/dist");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // to allow to use PWA
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            provider.Mappings[".webmanifest"] = "application/manifest+json";

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            } else
            {
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseCors("CorsPolicy");
            app.UseStaticFiles(new StaticFileOptions{ContentTypeProvider = provider});
            app.UseSpaStaticFiles(new StaticFileOptions{ContentTypeProvider = provider});
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapHealthChecksUI(options =>
                {
                    options.PageTitle = "Identity Admin SPA Health Checks";
                    options.AsideMenuOpened = false;
                    options.UIPath = "/hc";
                    options.ApiPath = "/hc-api";
                });
            });
            app.UseSpa(o => o.Options.SourcePath = "Client/dist");
        }

        private void ConfigureHealthChecks(IServiceCollection services, AppSettings settings)
        {
            // Health check endpoint for current service
            services
                .AddHealthChecks()
                .AddCheck("Listening on", () => HealthCheckResult.Healthy(this.Configuration["ASPNETCORE_URLS"]));

            // Configure UI, add all endpoints created in the cluster
            services.AddHealthChecksUI(opts =>
            {
                var spaInternal = settings.HBGIDENTITYADMINSPA;
                opts.AddHealthCheckEndpoint("Self", spaInternal + "/health");
                var identityInternal = settings.HBGIDENTITY;//.Replace("https", "http");
                opts.AddHealthCheckEndpoint("API.Identity: " + identityInternal, identityInternal + "/health");
                var adminApiInternal = settings.HBGIDENTITYADMINAPI;//.Replace("https", "http");
                opts.AddHealthCheckEndpoint("API.Identity.Admin.Api: " + adminApiInternal, adminApiInternal + "/health");
            }).AddInMemoryStorage();
        }
    }
}
