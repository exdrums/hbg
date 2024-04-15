using System.Threading.Tasks;
using Common.Utils;
using Projects.Dtos;

namespace Projects.WebSocket;

public interface IProjectsDataStoreServerActions
{
    Task LoadProject(DevExtremeLoadOptions loadOptions, long? projectId);
    Task InsertProject(ProjectDto value, long? projectId);
    Task UpdateProject(int key, ProjectDto values, long? projectId);
    Task RemoveProject(int key, long? projectId);
}
