using API.Identity.AuditLogging.Events;
using API.Identity.Admin.BusinessLogic.Dtos.Configuration;

namespace API.Identity.Admin.BusinessLogic.Events.IdentityResource
{
    public class IdentityResourceRequestedEvent : AuditEvent
    {
        public IdentityResourceDto IdentityResource { get; set; }

        public IdentityResourceRequestedEvent(IdentityResourceDto identityResource)
        {
            IdentityResource = identityResource;
        }
    }
}