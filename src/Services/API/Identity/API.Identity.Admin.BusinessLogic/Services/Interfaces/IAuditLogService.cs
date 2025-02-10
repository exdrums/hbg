using System;
using System.Threading.Tasks;
using API.Identity.Admin.BusinessLogic.Dtos.Log;

namespace API.Identity.Admin.BusinessLogic.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<AuditLogsDto> GetAsync(AuditLogFilterDto filters);

        Task DeleteLogsOlderThanAsync(DateTime deleteOlderThan);
    }
}
