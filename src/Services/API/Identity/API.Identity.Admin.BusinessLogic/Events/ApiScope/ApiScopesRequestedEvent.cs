using API.Identity.AuditLogging.Events;
using API.Identity.Admin.BusinessLogic.Dtos.Configuration;

namespace API.Identity.Admin.BusinessLogic.Events.ApiScope
{
    public class ApiScopesRequestedEvent : AuditEvent
    {
        public ApiScopesDto ApiScope { get; set; }

        public ApiScopesRequestedEvent(ApiScopesDto apiScope)
        {
            ApiScope = apiScope;
        }
    }
}