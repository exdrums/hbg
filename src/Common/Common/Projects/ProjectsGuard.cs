using Microsoft.EntityFrameworkCore;

namespace Common.Projects;

public class ProjectGuardDbContext<T> : DbContext where T : class, IProjectPermission
{
    public ProjectGuardDbContext(DbContextOptions options) : base(options) {}
    protected DbSet<T> ProjectPermission { get; set; }

    public bool Can(PermissionType type, int projectID) => ProjectPermission.Any(p => p.ProjectID == projectID && p.Type == type);
}

public interface IProjectPermission
{
    long ProjectPermissionID { get; set; }

    int ProjectID { get; set; }

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