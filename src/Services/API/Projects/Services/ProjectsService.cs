using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Exceptions;
using Common.Projects;
using Common.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Projects.Dtos;
using Projects.Models;
using Projects.WebSocket;

namespace Projects.Services;

public class ProjectsService
{
    private readonly ProjectsDbContext context;
    private readonly IHubContext<ProjectsHub, IProjectsDataStoreClientActions> hub;
    private readonly IMapper mapper;

    public ProjectsService(
        ProjectsDbContext context,
        IHubContext<ProjectsHub, IProjectsDataStoreClientActions> hub,
        IMapper mapper
    ) {
        this.context = context;
        this.hub = hub;
        this.mapper = mapper;
    }

    /// <summary>
    /// Get Project by id to work with
    /// Includes Articles, Plans
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<Project> GetProject(int projectId) => await context.Projects
        .Include(p => p.Articles)
        .Include(p => p.Plans)
        .FirstOrDefaultAsync(p => p.ProjectID == projectId)
        ?? throw new NotFoundException($"Projects not found {projectId}");


    /// <summary>
    /// Get all projects visible for loggedin user
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public IQueryable<ProjectDto> GetProjects(string userId) => context.Projects
        .Include(p => p.ProjectPermissions)
        .Where(p => p.ProjectPermissions.Any(perm => perm.UserID == userId /* && perm.Type == PermissionType.Read*/))
        .ProjectTo<ProjectDto>(mapper.ConfigurationProvider);


    /// <summary>
    /// Create new project and add default creator permissions
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<ProjectDto> CreateNewProject(ProjectDto dto, string userId)
    {
        var toCreate = mapper.Map<Project>(dto);

        var read = new ProjectPermission
        {
            Project = toCreate,
            UserID = userId,
            Type = PermissionType.Read
        };
        var update = new ProjectPermission
        {
            Project = toCreate,
            UserID = userId,
            Type = PermissionType.Update
        };
        var delete = new ProjectPermission
        {
            Project = toCreate,
            UserID = userId,
            Type = PermissionType.Delete
        };
        var share = new ProjectPermission
        {
            Project = toCreate,
            UserID = userId,
            Type = PermissionType.Share
        };
        var seepr = new ProjectPermission
        {
            Project = toCreate,
            UserID = userId,
            Type = PermissionType.SeePrices
        };

        toCreate.ProjectPermissions.Add(read);
        toCreate.ProjectPermissions.Add(update);
        toCreate.ProjectPermissions.Add(delete);
        toCreate.ProjectPermissions.Add(share);
        toCreate.ProjectPermissions.Add(seepr);

        var entity = await context.Projects.AddAsync(toCreate);
        await context.SaveChangesAsync();

        var result = mapper.Map<ProjectDto>(entity.Entity);

        return result;
    }

    /// <summary>
    /// Update Project by given ID and ProjectDto with changed fields
    /// </summary>
    /// <param name="projectId">ID of the project existing in database</param>
    /// <param name="dto">DTO with updated fields. Setup ignored fields in the AutoMapper profile.</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<ProjectDto> UpdateProject(int projectId, ProjectDto dto)
    {
        var proj = await context.Projects.FirstOrDefaultAsync(p => p.ProjectID == projectId);
        if (proj is null) throw new KeyNotFoundException("No project found by given id");

        mapper.Map(dto, proj);
        await context.SaveChangesAsync();

        return mapper.Map<ProjectDto>(proj);
    }

    /// <summary>
    /// Remove existing Project by ProjectID
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task RemoveProject(int projectId)
    {
        var proj = await context.Projects.FirstOrDefaultAsync(p => p.ProjectID == projectId);
        if (proj is null) throw new KeyNotFoundException("Cannot remove project by given id");

        context.Projects.Remove(proj);
        await context.SaveChangesAsync();
    }
}
