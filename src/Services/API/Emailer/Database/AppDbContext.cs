using API.Emailer.Models;
using Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace API.Emailer.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options) { }

    public DbSet<Template> Templates { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Use it ONLY to create Migrations
        // optionsBuilder.UseNpgsql("Server=localhost;port=3306; Database=hbgprojectsdb; Uid=hbg-dbuser; Pwd=hbg-password-database");
    }

    public async Task<List<Template>> GetTemplateListByUserId(string userId) =>
        await this.Templates
            .Where(t => t.UserID == userId)
            .ToListAsync();


    public async Task<Template> GetTemplateByUserId(string userId) =>
        await this.Templates
            .FirstOrDefaultAsync(t => t.UserID == userId)
            ?? throw new NotFoundException("Template not found.");
}

