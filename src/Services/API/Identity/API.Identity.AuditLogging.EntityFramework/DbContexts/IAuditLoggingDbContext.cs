using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using API.Identity.AuditLogging.EntityFramework.Entities;

namespace API.Identity.AuditLogging.EntityFramework.DbContexts
{
    public interface IAuditLoggingDbContext<TAuditLog> where TAuditLog : AuditLog
    {
        DbSet<TAuditLog> AuditLog { get; set; }

        Task<int> SaveChangesAsync();
    }
}