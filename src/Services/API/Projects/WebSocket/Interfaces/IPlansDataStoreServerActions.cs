using System.Threading.Tasks;
using Common.Utils;
using DevExtreme.AspNet.Data.ResponseModel;
using Projects.Dtos;

namespace Projects.WebSocket.Interfaces;

public interface IPlansDataStoreServerActions
{
    Task<LoadResult> LoadPlan(DevExtremeLoadOptions loadOptions, int projectId);
    Task<PlanDto> InsertPlan(PlanDto value, int projectId);
    Task UpdatePlan(int key, PlanDto values, int projectId);
    Task RemovePlan(int key, int projectId);
}
