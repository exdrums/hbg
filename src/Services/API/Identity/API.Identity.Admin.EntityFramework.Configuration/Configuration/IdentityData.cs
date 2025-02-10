using System.Collections.Generic;
using API.Identity.Admin.EntityFramework.Configuration.Configuration.Identity;

namespace API.Identity.Admin.EntityFramework.Configuration.Configuration
{
	public class IdentityData
    {
       public List<Role> Roles { get; set; }
       public List<User> Users { get; set; }
    }
}
