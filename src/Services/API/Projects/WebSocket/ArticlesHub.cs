using System.Threading.Tasks;
using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Projects.Dtos;
using Projects.WebSocket.Interfaces;

namespace Projects.WebSocket;

[Authorize]
public partial class ProjectsHub : IArticlesDataStoreServerActions
{
    public Task LoadArticle(DevExtremeLoadOptions loadOptions, long? permissionId)
    {
        throw new System.NotImplementedException();
    }

    public Task InsertArticle(ArticleDto value, long? permissionId)
    {
        throw new System.NotImplementedException();
    }

    public Task UpdateArticle(object key, object values, long? permissionId)
    {
        throw new System.NotImplementedException();
    }

    public Task RemoveArticle(object key, long? permissionId)
    {
        throw new System.NotImplementedException();
    }
}
