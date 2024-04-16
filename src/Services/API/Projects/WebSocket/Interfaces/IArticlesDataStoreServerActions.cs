using System.Threading.Tasks;
using Common.Utils;
using DevExtreme.AspNet.Data.ResponseModel;
using Projects.Dtos;

namespace Projects.WebSocket.Interfaces;

public interface IArticlesDataStoreServerActions
{
    Task<LoadResult> LoadArticle(DevExtremeLoadOptions loadOptions, int projectId);
    Task<ArticleDto> InsertArticle(ArticleDto value, int projectId);
    Task UpdateArticle(long key, ArticleDto values, int projectId);
    Task RemoveArticle(long key, int projectId);
}
