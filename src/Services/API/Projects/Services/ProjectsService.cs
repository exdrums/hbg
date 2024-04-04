using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Projects;
using Common.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Projects.Dtos;
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

    public async Task<IEnumerable<ProjectDto>> GetProjects(string userId, DevExtremeLoadOptions loadOptions)
    {
        var projects = await context.Projects
            .Include(p => p.ProjectPermissions)
            // .Where(p => p.ProjectPermissions.Any(perm => perm.UserID == userId /* && perm.Type == PermissionType.Read*/))
            .ProjectTo<ProjectDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        await hub.Clients.Group(userId).Loaded(projects);

        return projects;
    }

    public async Task<ProjectDto> CreateNewProject(ProjectDto dto, string userId)
    {
        var toCreate = mapper.Map<Project>(dto);

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

        toCreate.ProjectPermissions.Add(update);
        toCreate.ProjectPermissions.Add(delete);
        toCreate.ProjectPermissions.Add(share);
        toCreate.ProjectPermissions.Add(seepr);

        var entity = await context.Projects.AddAsync(toCreate);
        await context.SaveChangesAsync();

        var result = mapper.Map<ProjectDto>(entity.Entity);
        await hub.Clients.Group(userId).Added(result);

        return result;
    }
}
