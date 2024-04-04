using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Skoruba.AuditLogging.EntityFramework.Entities;
using API.Identity.Admin.Configuration.Database;
using API.Identity.Admin.EntityFramework.Shared.DbContexts;
using API.Identity.Admin.EntityFramework.Shared.Entities.Identity;
using API.Identity.Shared.Dtos;
using API.Identity.Shared.Dtos.Identity;
using API.Identity.Admin.Helpers;
using Skoruba.IdentityServer4.Shared.Configuration.Helpers;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace API.Identity.Admin;
public class Startup
{
    public Startup(IWebHostEnvironment env, IConfiguration configuration)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        HostingEnvironment = env;
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public IWebHostEnvironment HostingEnvironment { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Adds the IdentityServer4 Admin UI with custom options.
        services.AddIdentityServer4AdminUI<AdminIdentityDbContext, IdentityServerConfigurationDbContext, IdentityServerPersistedGrantDbContext,
        AdminLogDbContext, AdminAuditLogDbContext, AuditLog, IdentityServerDataProtectionDbContext,
            UserIdentity, UserIdentityRole, UserIdentityUserClaim, UserIdentityUserRole,
            UserIdentityUserLogin, UserIdentityRoleClaim, UserIdentityUserToken, string,
            IdentityUserDto, IdentityRoleDto, IdentityUsersDto, IdentityRolesDto, IdentityUserRolesDto,
            IdentityUserClaimsDto, IdentityUserProviderDto, IdentityUserProvidersDto, IdentityUserChangePasswordDto,
            IdentityRoleClaimsDto, IdentityUserClaimDto, IdentityRoleClaimDto>(ConfigureUIOptions);

        // Monitor changes in Admin UI views
        services.AddAdminUIRazorRuntimeCompilation(HostingEnvironment);

        // Add email senders which is currently setup for SendGrid and SMTP
        services.AddEmailSenders(Configuration);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        app.UseRouting();

        app.UseIdentityServer4AdminUI();

        app.UseEndpoints(endpoint =>
        {
            endpoint.MapIdentityServer4AdminUI();
            endpoint.MapIdentityServer4AdminUIHealthChecks();
        });
    }

    public virtual void ConfigureUIOptions(IdentityServer4AdminUIOptions options)
    {
        // Applies configuration from appsettings.
        options.BindConfiguration(Configuration);
        if (HostingEnvironment.IsDevelopment())
        {
            options.Security.UseDeveloperExceptionPage = true;
        }
        else
        {
            options.Security.UseHsts = true;
        }

        // Set migration assembly for application of db migrations
        var migrationsAssembly = MigrationAssemblyConfiguration.GetMigrationAssemblyByProvider(options.DatabaseProvider);
        options.DatabaseMigrations.SetMigrationsAssemblies(migrationsAssembly);

        // Use production DbContexts and auth services.
        options.Testing.IsStaging = false;
        options.HealthChecksBuilderFactory = services => services
            .AddHealthChecks()
            .AddCheck("Listening on", () => HealthCheckResult.Healthy(this.Configuration["ASPNETCORE_URLS"]));
    }
}
