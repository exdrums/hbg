﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using API.Identity.Admin.EntityFramework.Entities;
using API.Identity.Admin.EntityFramework.Extensions.Common;
using API.Identity.Admin.EntityFramework.Extensions.Enums;
using API.Identity.Admin.EntityFramework.Extensions.Extensions;
using API.Identity.Admin.EntityFramework.Interfaces;
using API.Identity.Admin.EntityFramework.Repositories.Interfaces;

namespace API.Identity.Admin.EntityFramework.Repositories
{
    public class LogRepository<TDbContext> : ILogRepository
        where TDbContext : DbContext, IAdminLogDbContext
    {
        protected readonly TDbContext DbContext;

        public LogRepository(TDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public virtual async Task DeleteLogsOlderThanAsync(DateTime deleteOlderThan)
        {
            var logsToDelete = await DbContext.Logs.Where(x => x.TimeStamp < deleteOlderThan.Date).ToListAsync();

            if(logsToDelete.Count == 0) return;

            DbContext.Logs.RemoveRange(logsToDelete);

            await AutoSaveChangesAsync();
        }

        public virtual async Task<PagedList<Log>> GetLogsAsync(string search, int page = 1, int pageSize = 10)
        {
            var pagedList = new PagedList<Log>();
            Expression<Func<Log, bool>> searchCondition = x => x.LogEvent.Contains(search) || x.Message.Contains(search) || x.Exception.Contains(search);
            var logs = await DbContext.Logs
                .WhereIf(!string.IsNullOrEmpty(search), searchCondition)                
                .PageBy(x => x.Id, page, pageSize)
                .ToListAsync();

            pagedList.Data.AddRange(logs);
            pagedList.PageSize = pageSize;
            pagedList.TotalCount = await DbContext.Logs.WhereIf(!string.IsNullOrEmpty(search), searchCondition).CountAsync();

            return pagedList;
        }

        protected virtual async Task<int> AutoSaveChangesAsync()
        {
            return AutoSaveChanges ? await DbContext.SaveChangesAsync() : (int)SavedStatus.WillBeSavedExplicitly;
        }

        public virtual async Task<int> SaveAllChangesAsync()
        {
            return await DbContext.SaveChangesAsync();
        }

        public bool AutoSaveChanges { get; set; } = true;
    }
}