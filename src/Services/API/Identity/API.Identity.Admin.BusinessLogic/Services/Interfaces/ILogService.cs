using System;
using System.Threading.Tasks;
using API.Identity.Admin.BusinessLogic.Dtos.Log;

namespace API.Identity.Admin.BusinessLogic.Services.Interfaces
{
    public interface ILogService
    {
        Task<LogsDto> GetLogsAsync(string search, int page = 1, int pageSize = 10);

        Task DeleteLogsOlderThanAsync(DateTime deleteOlderThan);
    }
}