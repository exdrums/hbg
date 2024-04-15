using System.Threading.Tasks;
using Common.Utils;
using Projects.Dtos;

namespace Projects.WebSocket.Interfaces;

public interface IArticlesDataStoreServerActions
{
    Task LoadArticle(DevExtremeLoadOptions loadOptions, long? projectId);
    Task InsertArticle(ArticleDto value, long? projectId);
    Task UpdateArticle(object key, object values, long? projectId);
    Task RemoveArticle(object key, long? projectId);
    
}
