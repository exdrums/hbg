﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using API.Identity.Admin.EntityFramework.Shared.DbContexts;
using API.Identity.Helpers;

namespace API.Identity.Configuration.Test;
public class StartupTest : Startup
{
    public StartupTest(IWebHostEnvironment environment, IConfiguration configuration) : base(environment, configuration)
    {
    }

    public override void RegisterDbContexts(IServiceCollection services)
    {
        services.RegisterDbContextsStaging<AdminIdentityDbContext, IdentityServerConfigurationDbContext, IdentityServerPersistedGrantDbContext, IdentityServerDataProtectionDbContext>();
    }
}
