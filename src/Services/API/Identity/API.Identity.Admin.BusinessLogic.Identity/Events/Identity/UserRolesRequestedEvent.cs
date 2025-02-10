using API.Identity.AuditLogging.Events;

namespace API.Identity.Admin.BusinessLogic.Identity.Events.Identity
{
    public class UserRolesRequestedEvent<TUserRolesDto> : AuditEvent
    {
        public TUserRolesDto Roles { get; set; }

        public UserRolesRequestedEvent(TUserRolesDto roles)
        {
            Roles = roles;
        }
    }
}