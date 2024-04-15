using System.Threading.Tasks;
using Common.Utils;
using Common.WebSocket;
using Projects.Dtos;

namespace Projects.WebSocket.Interfaces;

public interface IPlansDataStoreServerActions
{
    Task LoadPlan(DevExtremeLoadOptions loadOptions, long? projectId);
    Task InsertPlan(PlanDto value, long? projectId);
    Task UpdatePlan(object key, object values, long? projectId);
    Task RemovePlan(object key, long? projectId);
}
