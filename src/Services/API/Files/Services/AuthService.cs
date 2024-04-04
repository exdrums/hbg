using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace API.Files.Services
{
    public class AuthService
    {
        private readonly IHttpContextAccessor context;

        public AuthService(IHttpContextAccessor context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Returns ID of currenty loggedIn user
        /// </summary>
        /// <returns></returns>
        public string GetUserId() => context.HttpContext.User.FindFirst("sub")?.Value ?? "null";

        /// <summary>
        /// Get list of all accesible profiles for the user (TODO: get it from token)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllProfiles() => context.HttpContext.User.FindAll("role")
            .Where(x => x.Value.StartsWith("svc-"))
            .Select(x => x.Value);
    }
}