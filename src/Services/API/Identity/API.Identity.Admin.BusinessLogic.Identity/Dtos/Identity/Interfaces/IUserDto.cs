﻿using System;

namespace API.Identity.Admin.BusinessLogic.Identity.Dtos.Identity.Interfaces
{
    public interface IUserDto : IBaseUserDto
    {
        string UserName { get; set; }
        string Email { get; set; }
        bool EmailConfirmed { get; set; }
        string PhoneNumber { get; set; }
        bool PhoneNumberConfirmed { get; set; }
        bool LockoutEnabled { get; set; }
        bool TwoFactorEnabled { get; set; }
        int AccessFailedCount { get; set; }
        DateTimeOffset? LockoutEnd { get; set; }
    }
}
