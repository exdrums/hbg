using Microsoft.EntityFrameworkCore;

namespace Common.Projects;

public class ProjectGuardDbContext<T> : DbContext where T : class, IProjectPermission
{
    public ProjectGuardDbContext(DbContextOptions options) : base(options) {}
    protected DbSet<T> ProjectPermissions { get; set; }

    public bool Can(PermissionType type, int projectID) => ProjectPermissions.Any(p => p.ProjectID == projectID && p.Type == type);
    public async Task<List<T>> GetForUserAsync(string userId) => await ProjectPermissions.Where(pp => pp.UserID == userId).ToListAsync();
}

public interface IProjectPermission
{
    long ProjectPermissionID { get; set; }

    int ProjectID { get; set; }
    string UserID { get; set; }

    PermissionType Type { get; set; }
}

public enum PermissionType : byte
{
    None,
    Read,
    Update,
    Delete,
    Share,
    SeePrices,

}