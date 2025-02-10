using API.Identity.AuditLogging.Events;
using API.Identity.Admin.BusinessLogic.Identity.Dtos.Identity;

namespace API.Identity.Admin.BusinessLogic.Identity.Events.Identity
{
    public class RoleUsersRequestedEvent<TUsersDto> : AuditEvent
    {
        public TUsersDto Users { get; set; }

        public RoleUsersRequestedEvent(TUsersDto users)
        {
            Users = users;
        }
    }
}