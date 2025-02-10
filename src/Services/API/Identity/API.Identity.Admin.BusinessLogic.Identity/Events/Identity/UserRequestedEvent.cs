using API.Identity.AuditLogging.Events;
using API.Identity.Admin.BusinessLogic.Identity.Dtos.Identity;

namespace API.Identity.Admin.BusinessLogic.Identity.Events.Identity
{
    public class UserRequestedEvent<TUserDto> : AuditEvent
    {
        public TUserDto UserDto { get; set; }

        public UserRequestedEvent(TUserDto userDto)
        {
            UserDto = userDto;
        }
    }
}