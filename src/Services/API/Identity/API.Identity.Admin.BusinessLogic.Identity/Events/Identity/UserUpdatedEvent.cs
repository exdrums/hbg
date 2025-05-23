﻿using API.Identity.AuditLogging.Events;

namespace API.Identity.Admin.BusinessLogic.Identity.Events.Identity
{
    public class UserUpdatedEvent<TUserDto> : AuditEvent
    {
        public TUserDto OriginalUser { get; set; }
        public TUserDto User { get; set; }

        public UserUpdatedEvent(TUserDto originalUser, TUserDto user)
        {
            OriginalUser = originalUser;
            User = user;
        }
    }
}