using System;
using System.Threading.Tasks;
using API.Identity.AuditLogging.EntityFramework.Entities;
using API.Identity.AuditLogging.EntityFramework.Mapping;
using API.Identity.AuditLogging.EntityFramework.Repositories;
using API.Identity.AuditLogging.Events;
using API.Identity.AuditLogging.Services;

namespace API.Identity.AuditLogging.EntityFramework.Services
{
    public class DatabaseAuditEventLoggerSink<TAuditLog> : IAuditEventLoggerSink 
        where TAuditLog : AuditLog, new()
    {
        private readonly IAuditLoggingRepository<TAuditLog> _auditLoggingRepository;

        public DatabaseAuditEventLoggerSink(IAuditLoggingRepository<TAuditLog> auditLoggingRepository)
        {
            _auditLoggingRepository = auditLoggingRepository;
        }

        public virtual async Task PersistAsync(AuditEvent auditEvent)
        {
            if (auditEvent == null) throw new ArgumentNullException(nameof(auditEvent));

            var auditLog = auditEvent.MapToEntity<TAuditLog>();

            await _auditLoggingRepository.SaveAsync(auditLog);
        }
    }
}