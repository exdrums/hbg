using System.ComponentModel.DataAnnotations;
using API.Identity.Admin.BusinessLogic.Identity.Dtos.Identity.Base;
using API.Identity.Admin.BusinessLogic.Identity.Dtos.Identity.Interfaces;

namespace API.Identity.Admin.BusinessLogic.Identity.Dtos.Identity
{
    public class RoleDto<TKey> : BaseRoleDto<TKey>, IRoleDto
    {      
        [Required]
        public string Name { get; set; }
    }
}