using Microsoft.Extensions.DependencyInjection;

namespace API.Identity.AuditLogging.Extensions
{
    public interface IAuditLoggingBuilder
    {
        IServiceCollection Services { get; }
    }
}