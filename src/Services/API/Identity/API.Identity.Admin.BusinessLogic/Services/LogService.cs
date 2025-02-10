using System;
using System.Threading.Tasks;
using API.Identity.AuditLogging.Services;
using API.Identity.Admin.BusinessLogic.Dtos.Log;
using API.Identity.Admin.BusinessLogic.Events.Log;
using API.Identity.Admin.BusinessLogic.Mappers;
using API.Identity.Admin.BusinessLogic.Services.Interfaces;
using API.Identity.Admin.EntityFramework.Repositories.Interfaces;

namespace API.Identity.Admin.BusinessLogic.Services
{
    public class LogService : ILogService
    {
        protected readonly ILogRepository Repository;
        protected readonly IAuditEventLogger AuditEventLogger;

        public LogService(ILogRepository repository, IAuditEventLogger auditEventLogger)
        {
            Repository = repository;
            AuditEventLogger = auditEventLogger;
        }

        public virtual async Task<LogsDto> GetLogsAsync(string search, int page = 1, int pageSize = 10)
        {
            var pagedList = await Repository.GetLogsAsync(search, page, pageSize);
            var logs = pagedList.ToModel();

            await AuditEventLogger.LogEventAsync(new LogsRequestedEvent());

            return logs;
        }

        public virtual async Task DeleteLogsOlderThanAsync(DateTime deleteOlderThan)
        {
            await Repository.DeleteLogsOlderThanAsync(deleteOlderThan);

            await AuditEventLogger.LogEventAsync(new LogsDeletedEvent(deleteOlderThan));
        }
    }
}
