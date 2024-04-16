using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Common.Exceptions;
using Common.Projects;
using Common.Utils;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Projects.Dtos;
using Projects.Models;
using Projects.WebSocket.Interfaces;

namespace Projects.WebSocket;

[Authorize]
public partial class ProjectsHub : IPlansDataStoreServerActions
{
    public async Task<LoadResult> LoadPlan(DevExtremeLoadOptions loadOptions, int projectId)
    {
        await AuthorizeProject(projectId, ProjectOperations.Read);
        var project = await service.GetProject(projectId);

        var qarticles = project.Plans.AsQueryable().ProjectTo<PlanDto>(mapper.ConfigurationProvider);

        return DataSourceLoader.Load(qarticles, loadOptions);
    }

    public async Task<PlanDto> InsertPlan(PlanDto value, int projectId)
    {
        await AuthorizeProject(projectId, ProjectOperations.Update);
        var project = await service.GetProject(projectId);

        var toCreate = mapper.Map<Plan>(value);

        project.Plans.Add(toCreate);
        await dbContext.SaveChangesAsync();

        var result = mapper.Map<PlanDto>(toCreate);
        return result;
    }

    public async Task UpdatePlan(int key, PlanDto values, int projectId)
    {
        await AuthorizeProject(projectId, ProjectOperations.Update);
        var project = await service.GetProject(projectId);

        var toUpdate = project.Plans.FirstOrDefault(a => a.PlanID == key) ?? throw new NotFoundException($"Plan with key {key} not found in the project {projectId}");

        mapper.Map(values, toUpdate);

        await dbContext.SaveChangesAsync();
    }

    public async Task RemovePlan(int key, int projectId)
    {
        await AuthorizeProject(projectId, ProjectOperations.Update);
        var project = await service.GetProject(projectId);

        var toRemove = project.Plans.FirstOrDefault(a => a.PlanID == key) ?? throw new NotFoundException($"Plan with key {key} not found in the project {projectId}");

        dbContext.Plans.Remove(toRemove);
        await dbContext.SaveChangesAsync();
    }
}
