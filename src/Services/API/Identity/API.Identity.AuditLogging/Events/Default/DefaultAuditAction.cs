namespace API.Identity.AuditLogging.Events.Default
{
    public class DefaultAuditAction : IAuditAction
    {
        public object Action { get; set; }
    }
}