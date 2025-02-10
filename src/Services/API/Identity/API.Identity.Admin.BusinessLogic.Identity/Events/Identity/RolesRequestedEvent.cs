using API.Identity.AuditLogging.Events;
namespace API.Identity.Admin.BusinessLogic.Identity.Events.Identity
{
    public class RolesRequestedEvent<TRolesDto> : AuditEvent
    {
        public TRolesDto Roles { get; set; }

        public RolesRequestedEvent(TRolesDto roles)
        {
            Roles = roles;
        }
    }
}