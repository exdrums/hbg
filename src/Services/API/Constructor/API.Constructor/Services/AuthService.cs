using System;
using Microsoft.AspNetCore.Http;

namespace API.Constructor.Services
{
    public class AuthService
    {
        private readonly IHttpContextAccessor _context;

        public AuthService(IHttpContextAccessor context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Returns ID of currently logged in user
        /// </summary>
        public string GetUserId() => _context.HttpContext.User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException("User ID not found in token");

        /// <summary>
        /// Returns username of currently logged in user
        /// </summary>
        public string GetUserName() => _context.HttpContext.User.Identity?.Name ?? "Unknown";

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        public bool IsAuthenticated() => _context.HttpContext.User.Identity?.IsAuthenticated ?? false;
    }
}
