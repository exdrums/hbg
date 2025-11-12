using Microsoft.EntityFrameworkCore;
using API.Constructor.Models.Entities;

namespace API.Constructor.Data
{
    public class ConstructorDbContext : DbContext
    {
        public ConstructorDbContext(DbContextOptions<ConstructorDbContext> options)
            : base(options)
        {
        }

        public DbSet<ConstructorProject> ConstructorProjects { get; set; }
        public DbSet<ProjectConfiguration> ProjectConfigurations { get; set; }
        public DbSet<GeneratedImage> GeneratedImages { get; set; }
        public DbSet<ChatInteraction> ChatInteractions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ConstructorProject
            modelBuilder.Entity<ConstructorProject>(entity =>
            {
                entity.HasKey(e => e.ProjectId);
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_ConstructorProjects_UserId");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_ConstructorProjects_CreatedAt");
                entity.HasIndex(e => new { e.UserId, e.CreatedAt }).HasDatabaseName("IX_ConstructorProjects_UserId_CreatedAt");

                entity.HasMany(e => e.Configurations)
                    .WithOne(e => e.Project)
                    .HasForeignKey(e => e.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.ChatInteractions)
                    .WithOne(e => e.Project)
                    .HasForeignKey(e => e.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ProjectConfiguration
            modelBuilder.Entity<ProjectConfiguration>(entity =>
            {
                entity.HasKey(e => e.ConfigurationId);
                entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_ProjectConfigurations_ProjectId");
                entity.HasIndex(e => new { e.ProjectId, e.UpdatedAt }).HasDatabaseName("IX_ProjectConfigurations_ProjectId_UpdatedAt");

                entity.HasMany(e => e.GeneratedImages)
                    .WithOne(e => e.Configuration)
                    .HasForeignKey(e => e.ConfigurationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // GeneratedImage
            modelBuilder.Entity<GeneratedImage>(entity =>
            {
                entity.HasKey(e => e.ImageId);
                entity.HasIndex(e => e.ConfigurationId).HasDatabaseName("IX_GeneratedImages_ConfigurationId");
                entity.HasIndex(e => new { e.ConfigurationId, e.GeneratedAt }).HasDatabaseName("IX_GeneratedImages_ConfigurationId_GeneratedAt");
                entity.HasIndex(e => e.IsDeleted).HasDatabaseName("IX_GeneratedImages_IsDeleted");
            });

            // ChatInteraction
            modelBuilder.Entity<ChatInteraction>(entity =>
            {
                entity.HasKey(e => e.InteractionId);
                entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_ChatInteractions_ProjectId");
                entity.HasIndex(e => new { e.ProjectId, e.CreatedAt }).HasDatabaseName("IX_ChatInteractions_ProjectId_CreatedAt");

                entity.HasOne(e => e.ResultingImage)
                    .WithMany()
                    .HasForeignKey(e => e.ResultingImageId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
