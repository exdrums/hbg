using System;
using System.Threading.Tasks;
using Common.Projects;
using Common.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Projects.Dtos;
using Projects.Services;

namespace Projects.WebSocket;

[Authorize]
public partial class ProjectsHub : Hub<IProjectsDataStoreClientActions>, IProjectsDataStoreServerActions
{
    private readonly ProjectsDbContext dbContext;
    private readonly ProjectsService service;
    private readonly IAuthorizationService authorizationService;
    private readonly IAuthenticationService authenticationService;

    public ProjectsHub(
        ProjectsDbContext dbContext,
        ProjectsService service,
        IAuthorizationService authorizationService,
        IAuthenticationService authenticationService
    ) {
        this.authorizationService = authorizationService;
        this.authenticationService = authenticationService;
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

    public async Task<ProjectDto> ByKey(int projectID) 
    {
        var authResult = await authorizationService.AuthorizeAsync(this.Context.User, projectID, ProjectOperations.Read);
        if (!authResult.Succeeded) throw new Exception("AuthorizationException");

        return null;
    }

    public async Task LoadProject(DevExtremeLoadOptions? loadOptions, long? permissionId)
    {
        var projects = await service.GetProjects(Context.UserIdentifier, loadOptions);
        await Clients.Group(Context.UserIdentifier).LoadedProject(projects);
    }

    public async Task InsertProject(ProjectDto value, long? permissionId)
    {
        var created = await service.CreateNewProject(value, Context.UserIdentifier);

        await Clients.Group(Context.UserIdentifier).AddedProject(created);
    }

    public async Task UpdateProject(int projectId, ProjectDto values, long? permissionId)
    {
        await AuthorizeProject(projectId, ProjectOperations.Update);
        await service.UpdateProject(projectId, values);
    }

    public async Task RemoveProject(int projectId, long? permissionId)
    {
        await AuthorizeProject(projectId, ProjectOperations.Delete);
        await service.RemoveProject(projectId);
    }

    protected async Task AuthorizeProject(int projectId, OperationAuthorizationRequirement op) 
    {
        await authenticationService.AuthenticateAsync(Context.GetHttpContext(), null);
        var authResult = await authorizationService.AuthorizeAsync(Context.User, projectId, op);
        if (!authResult.Succeeded) throw new UnauthorizedAccessException("Unauthorized access to the project");
    }

    // Share
}
