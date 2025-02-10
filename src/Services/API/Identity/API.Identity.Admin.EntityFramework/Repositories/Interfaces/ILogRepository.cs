using System;
using System.Threading.Tasks;
using API.Identity.Admin.EntityFramework.Entities;
using API.Identity.Admin.EntityFramework.Extensions.Common;

namespace API.Identity.Admin.EntityFramework.Repositories.Interfaces
{
    public interface ILogRepository
    {
        Task<PagedList<Log>> GetLogsAsync(string search, int page = 1, int pageSize = 10);

        Task DeleteLogsOlderThanAsync(DateTime deleteOlderThan);

        bool AutoSaveChanges { get; set; }
    }
}