﻿using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using API.Identity.Admin.UI.Configuration;
using API.Identity.Admin.UI.Helpers;
using API.Identity.Admin.UI.Middlewares;
using System;
using System.Collections.Generic;
using API.Identity.Admin.UI.Configuration.Constants;

namespace Microsoft.AspNetCore.Builder
{
    public static class AdminUIApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds the Admin UI to the pipeline of this application. This method must be called 
        /// between UseRouting() and UseEndpoints().
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseIdentityServer4AdminUI(this IApplicationBuilder app)
        {
            app.UseRoutingDependentMiddleware(app.ApplicationServices.GetRequiredService<TestingConfiguration>());

            return app;
        }

        /// <summary>
        /// Maps the Admin UI to the routes of this application.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="patternPrefix"></param>
        public static IEndpointConventionBuilder MapIdentityServer4AdminUI(this IEndpointRouteBuilder endpoint, string patternPrefix = "/")
        {
            return endpoint.MapAreaControllerRoute(CommonConsts.AdminUIArea, CommonConsts.AdminUIArea, patternPrefix + "{controller=Home}/{action=Index}/{id?}");
        }

        /// <summary>
        /// Maps the Admin UI health checks to the routes of this application.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="pattern"></param>
        public static IEndpointConventionBuilder MapIdentityServer4AdminUIHealthChecks(this IEndpointRouteBuilder endpoint, string pattern = "/health", Action<HealthCheckOptions> configureAction = null)
        {
            var options = new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            };

            configureAction?.Invoke(options);

            return endpoint.MapHealthChecks(pattern, options);
        }
    }
}
