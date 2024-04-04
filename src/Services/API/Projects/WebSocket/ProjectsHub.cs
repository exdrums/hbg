using System;
using System.Threading.Tasks;
using Common.Projects;
using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Projects.Dtos;
using Projects.Services;

namespace Projects.WebSocket;

[Authorize]
public class ProjectsHub : Hub<IProjectsDataStoreClientActions>, IProjectsDataStoreServerActions
{
    private readonly ProjectsDbContext dbContext;
    private readonly ProjectsService service;
    private readonly IAuthorizationService authorizationService;
    public ProjectsHub(
        ProjectsDbContext dbContext,
        ProjectsService service,
        IAuthorizationService authorizationService
    ) {
        this.authorizationService = authorizationService;
        this.dbContext = dbContext;
        this.service = service;
    }

    public override async Task OnConnectedAsync()
    {
        // add group with the userId
		await Groups.AddToGroupAsync(Context.ConnectionId, Context.UserIdentifier);
        await base.OnConnectedAsync();
    }

	public async override Task OnDisconnectedAsync(Exception exception)
	{
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.UserIdentifier);
		await base.OnDisconnectedAsync(exception);
	}

    public async Task<ProjectInfoDto> ByKey(int projectID) 
    {
        var authResult = await authorizationService.AuthorizeAsync(this.Context.User, projectID, ProjectOperations.Read);
        if (!authResult.Succeeded) throw new Exception("AuthorizationException");

        return null;
    }

    public async Task Load(DevExtremeLoadOptions loadOptions)
    {
        await service.GetProjects(Context.UserIdentifier, loadOptions);
    }

    public async Task Insert(ProjectDto value)
    {
        await service.CreateNewProject(value, Context.UserIdentifier);
    }

    public Task Update(object key, object values)
    {
        throw new NotImplementedException();
    }

    public Task Remove(object key)
    {
        throw new NotImplementedException();
    }

    // Share
}
