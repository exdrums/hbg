using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using API.Constructor.Data;
using API.Constructor.Models.Entities;

namespace API.Constructor.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ConstructorDbContext _context;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(ConstructorDbContext context, ILogger<ProjectService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ConstructorProject>> GetUserProjectsAsync(string userId)
        {
            _logger.LogInformation("Getting projects for user {UserId}", userId);

            return await _context.ConstructorProjects
                .Where(p => p.UserId == userId && p.IsActive)
                .OrderByDescending(p => p.UpdatedAt)
                .ToListAsync();
        }

        public async Task<ConstructorProject> GetProjectAsync(Guid projectId, string userId)
        {
            _logger.LogInformation("Getting project {ProjectId} for user {UserId}", projectId, userId);

            var project = await _context.ConstructorProjects
                .Include(p => p.Configurations)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.UserId == userId);

            if (project == null)
            {
                throw new UnauthorizedAccessException($"Project {projectId} not found or access denied");
            }

            return project;
        }

        public async Task<ConstructorProject> CreateProjectAsync(ConstructorProject project)
        {
            _logger.LogInformation("Creating project for user {UserId}", project.UserId);

            _context.ConstructorProjects.Add(project);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Project created with ID: {ProjectId}", project.ProjectId);

            return project;
        }

        public async Task<ConstructorProject> UpdateProjectAsync(Guid projectId, ConstructorProject updatedProject, string userId)
        {
            _logger.LogInformation("Updating project {ProjectId}", projectId);

            var project = await GetProjectAsync(projectId, userId);

            project.Name = updatedProject.Name ?? project.Name;
            project.Description = updatedProject.Description ?? project.Description;
            project.IsActive = updatedProject.IsActive;
            project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Project {ProjectId} updated", projectId);

            return project;
        }

        public async Task<bool> DeleteProjectAsync(Guid projectId, string userId)
        {
            _logger.LogInformation("Deleting project {ProjectId}", projectId);

            var project = await GetProjectAsync(projectId, userId);

            project.IsActive = false;
            project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Project {ProjectId} marked as inactive", projectId);

            return true;
        }

        public async Task<List<GeneratedImage>> GetProjectImagesAsync(Guid projectId, string userId)
        {
            _logger.LogInformation("Getting images for project {ProjectId}", projectId);

            var project = await GetProjectAsync(projectId, userId);

            return await _context.GeneratedImages
                .Include(i => i.Configuration)
                .Where(i => i.Configuration.ProjectId == projectId && !i.IsDeleted)
                .OrderByDescending(i => i.GeneratedAt)
                .ToListAsync();
        }
    }
}
