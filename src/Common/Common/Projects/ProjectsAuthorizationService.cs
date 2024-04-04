using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Common.Projects;

public static class ProjectOperations
{
    public static OperationAuthorizationRequirement Create = new OperationAuthorizationRequirement { Name = nameof(Create) };
    public static OperationAuthorizationRequirement Read = new OperationAuthorizationRequirement { Name = nameof(Read) };
    public static OperationAuthorizationRequirement Update = new OperationAuthorizationRequirement { Name = nameof(Update) };
    public static OperationAuthorizationRequirement Delete = new OperationAuthorizationRequirement { Name = nameof(Delete) };
    public static OperationAuthorizationRequirement Share = new OperationAuthorizationRequirement { Name = nameof(Share) };
    public static OperationAuthorizationRequirement SeePrices = new OperationAuthorizationRequirement { Name = nameof(SeePrices) };
}

/// <summary>
/// Add this to the Services:
/// builder.Services.AddTransient<IClaimsTransformation, ProjectOperationsClaims>();
/// </summary>
public class ProjectOperationsClaims<TContext, TPermission> : IClaimsTransformation where TPermission : class, IProjectPermission where TContext : ProjectGuardDbContext<TPermission> 
{
    private readonly TContext dbContext;

    public ProjectOperationsClaims(TContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        ClaimsIdentity projectAccessIdentity = new ClaimsIdentity();

        // Get userID from Principal

        // Get all ProjectPermissions anf create claim for each permission object
        var permissions = new List<IProjectPermission>();

        // Add claims:
        // project-read: 45
        // project-change: 45
        // project-share: 45
        // project-read: 50
        // project-read: 38
        foreach(var p in permissions) 
        {
            var claimName = p.Type switch
            {
                PermissionType.Read => "project-" + ProjectOperations.Read.Name.ToLower(),
                PermissionType.Update => "project-" + ProjectOperations.Update.Name.ToLower(),
                PermissionType.Delete => "project-" + ProjectOperations.Delete.Name.ToLower(),
                PermissionType.Share => "project-" + ProjectOperations.Share.Name.ToLower(),
                PermissionType.SeePrices => "project-" + ProjectOperations.SeePrices.Name.ToLower(),
                _ => throw new Exception("Unknown permission type found in database.")
            };
            projectAccessIdentity.AddClaim(new Claim(claimName, p.ProjectID.ToString()));
        }

        principal.AddIdentity(projectAccessIdentity);

        return principal;
    }
}



/// <summary>
/// Add this to the services:
/// builder.Services.AddSingleton<IAuthorizationHandler, ProjectAuthorizationOperationsHandler>();
/// </summary>
public class ProjectAuthorizationOperationsHandler : AuthorizationHandler<OperationAuthorizationRequirement, int>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, int resource)
    {
        var claimName = "project-" + requirement.Name.ToLower();
        if(context.User.HasClaim(claimName, resource.ToString())) 
        {
            context.Succeed(requirement);
        }
    }
}