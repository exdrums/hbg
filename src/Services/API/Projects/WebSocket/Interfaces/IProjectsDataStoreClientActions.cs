using System.Collections.Generic;
using System.Threading.Tasks;
using Common.WebSocket;
using Projects.Dtos;

namespace Projects.WebSocket;

public interface IProjectsDataStoreClientActions
{
    Task LoadedProject(List<ProjectDto> list);
    Task LoadedPlan(List<PlanDto> list);
    Task LoadedArticle(List<ArticleDto> list);
    Task AddedProject(ProjectDto item);
    Task AddedPlan(ArticleDto item);
    Task AddedArticle(ArticleDto item);
}
