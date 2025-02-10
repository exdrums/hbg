using Microsoft.Extensions.DependencyInjection;
using API.Identity.AuditLogging.EntityFramework.Extensions;

namespace API.Identity.AuditLogging.Extensions
{
    public class AuditLoggingBuilder : IAuditLoggingBuilder
    {
        public AuditLoggingBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}