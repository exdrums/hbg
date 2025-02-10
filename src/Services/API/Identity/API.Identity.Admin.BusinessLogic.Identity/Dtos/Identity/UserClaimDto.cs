using System.ComponentModel.DataAnnotations;
using API.Identity.Admin.BusinessLogic.Identity.Dtos.Identity.Base;
using API.Identity.Admin.BusinessLogic.Identity.Dtos.Identity.Interfaces;

namespace API.Identity.Admin.BusinessLogic.Identity.Dtos.Identity
{
    public class UserClaimDto<TKey> : BaseUserClaimDto<TKey>, IUserClaimDto
    {
        [Required]
        public string ClaimType { get; set; }

        [Required]
        public string ClaimValue { get; set; }
    }
}