using System.Threading.Tasks;
using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Projects.Dtos;
using Projects.WebSocket.Interfaces;

namespace Projects.WebSocket;

[Authorize]
public partial class ProjectsHub : IPlansDataStoreServerActions
{
    public Task LoadPlan(DevExtremeLoadOptions loadOptions, long? permissionId)
    {
        throw new System.NotImplementedException();
    }

    public Task InsertPlan(PlanDto value, long? permissionId)
    {
        throw new System.NotImplementedException();
    }

    public Task UpdatePlan(object key, object values, long? permissionId)
    {
        throw new System.NotImplementedException();
    }

    public Task RemovePlan(object key, long? permissionId)
    {
        throw new System.NotImplementedException();
    }
}
