using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Exceptions;
using Common.Projects;
using Common.Utils;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Projects.Dtos;
using Projects.Models;
using Projects.WebSocket.Interfaces;

namespace Projects.WebSocket;

[Authorize]
public partial class ProjectsHub : IArticlesDataStoreServerActions
{

    /// <summary>
    /// Load all Articles for DevExtreme grid only for selected project
    /// </summary>
    /// <param name="loadOptions"></param>
    /// 
    /// <param name="projectId"></param>
    /// <returns></returns>
    public async Task<LoadResult> LoadArticle(DevExtremeLoadOptions loadOptions, int projectId)
    {
        await AuthorizeProject(projectId, ProjectOperations.Read);
        var project = await service.GetProject(projectId);

        var qarticles = project.Articles.AsQueryable().ProjectTo<ArticleDto>(mapper.ConfigurationProvider);

        return DataSourceLoader.Load(qarticles, loadOptions);
    }

    public async Task<ArticleDto> InsertArticle(ArticleDto value, int projectId)
    {
        await AuthorizeProject(projectId, ProjectOperations.Update);
        var project = await service.GetProject(projectId);

        var toCreate = mapper.Map<Article>(value);

        project.Articles.Add(toCreate);
        await dbContext.SaveChangesAsync();

        var result = mapper.Map<ArticleDto>(toCreate);
        return result;
    }

    public async Task UpdateArticle(long key, ArticleDto values, int projectId)
    {
        await AuthorizeProject(projectId, ProjectOperations.Update);
        var project = await service.GetProject(projectId);

        var toUpdate = project.Articles.FirstOrDefault(a => a.ArticleID == key) ?? throw new NotFoundException($"Article with key {key} not found in the project {projectId}");

        mapper.Map(values, toUpdate);

        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveArticle(long key, int projectId)
    {
        await AuthorizeProject(projectId, ProjectOperations.Update);
        var project = await service.GetProject(projectId);

        var toRemove = project.Articles.FirstOrDefault(a => a.ArticleID == key) ?? throw new NotFoundException($"Article with key {key} not found in the project {projectId}");

        dbContext.Articles.Remove(toRemove);
        await dbContext.SaveChangesAsync();
    }
}
