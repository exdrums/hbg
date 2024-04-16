using System.Threading.Tasks;
using Common.Utils;
using DevExtreme.AspNet.Data.ResponseModel;
using Projects.Dtos;

namespace Projects.WebSocket;

public interface IProjectsDataStoreServerActions
{
    Task<LoadResult> LoadProject(DevExtremeLoadOptions loadOptions, long? projectId);
    Task<ProjectDto> InsertProject(ProjectDto value, long? projectId);
    Task UpdateProject(int key, ProjectDto values, long? projectId);
    Task RemoveProject(int key, long? projectId);
}
