using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SVC.Example.Data;
using SVC.Example.Model;

namespace SVC.Example
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddDbContext<Context>(options =>
            {
                options.UseInMemoryDatabase("svc-example");
            }, ServiceLifetime.Transient, ServiceLifetime.Transient);

            this.RegisterIdentity(services);
            this.RegisterCors(services);


            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            #region Seed database once host is up
            var context = app.ApplicationServices.GetService<Context>();
            context.Database.EnsureCreated();
            var mock = new MockData();
            // Fill context
            context.Locations.AddRange(mock.Locations);
            context.Orders.AddRange(mock.Orders);
            context.Products.AddRange(mock.Products);
            context.OrderedProducts.AddRange(mock.OrderedProducts);
            context.SaveChanges();

            #endregion

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors("CorsPolicy");

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireAuthorization("ApiScope");
            });
        }

        #region Private methods

        private void RegisterIdentity(IServiceCollection services)
        {
            var identityUrl = Configuration.GetValue<string>("HBGIDENTITY");

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = identityUrl;
                    options.Audience = Configuration.GetValue<string>("AUDIENCE");
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters.RoleClaimType = "role";
                });
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "svcs");
                    policy.RequireRole("svc-example");
                });
            });
        }

        private void RegisterCors(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    // .AllowAnyOrigin()
                    // .SetIsOriginAllowed((host) => true)
                    .WithOrigins(
                        Configuration.GetValue<string>("HBGSPA"),
                        Configuration.GetValue<string>("HBGSPADEV"))
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            });
        }

        #endregion
    }
}
