using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Constructor.Models.Entities;

namespace API.Constructor.Services
{
    public interface IProjectService
    {
        Task<List<ConstructorProject>> GetUserProjectsAsync(string userId);
        Task<ConstructorProject> GetProjectAsync(Guid projectId, string userId);
        Task<ConstructorProject> CreateProjectAsync(ConstructorProject project);
        Task<ConstructorProject> UpdateProjectAsync(Guid projectId, ConstructorProject project, string userId);
        Task<bool> DeleteProjectAsync(Guid projectId, string userId);
        Task<List<GeneratedImage>> GetProjectImagesAsync(Guid projectId, string userId);
    }
}
