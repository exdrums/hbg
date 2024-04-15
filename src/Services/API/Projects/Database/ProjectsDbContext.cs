using Common.Projects;
using Microsoft.EntityFrameworkCore;
using Projects.Models;

public class ProjectsDbContext : ProjectGuardDbContext<ProjectPermission>
{
    public ProjectsDbContext(DbContextOptions<ProjectsDbContext> options) : base(options) {}

    public DbSet<Project> Projects { get; set; }
    public DbSet<Plan> Plans { get; set; }
    public DbSet<Article> Articles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Use it ONLY to create Migrations
        // optionsBuilder.UseNpgsql("Server=localhost;port=3306; Database=hbgprojectsdb; Uid=hbg-dbuser; Pwd=hbg-password-database");
    }
}