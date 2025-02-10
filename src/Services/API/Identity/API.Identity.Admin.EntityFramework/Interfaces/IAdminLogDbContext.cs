using Microsoft.EntityFrameworkCore;
using API.Identity.Admin.EntityFramework.Entities;

namespace API.Identity.Admin.EntityFramework.Interfaces
{
    public interface IAdminLogDbContext
    {
        DbSet<Log> Logs { get; set; }
    }
}
