﻿using System.Collections.Generic;

namespace API.Identity.Admin.BusinessLogic.Identity.Dtos.Identity.Interfaces
{
    public interface IRolesDto
    {
        int PageSize { get; set; }
        int TotalCount { get; set; }
        List<IRoleDto> Roles { get; }
    }
}
