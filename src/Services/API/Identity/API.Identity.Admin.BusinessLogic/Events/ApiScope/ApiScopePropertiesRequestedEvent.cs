using API.Identity.AuditLogging.Events;
using API.Identity.Admin.BusinessLogic.Dtos.Configuration;

namespace API.Identity.Admin.BusinessLogic.Events.ApiScope
{
    public class ApiScopePropertiesRequestedEvent : AuditEvent
    {
        public ApiScopePropertiesRequestedEvent(int apiScopeId, ApiScopePropertiesDto apiScopeProperties)
        {
            ApiScopeId = apiScopeId;
            ApiResourceProperties = apiScopeProperties;
        }

        public int ApiScopeId { get; set; }
        public ApiScopePropertiesDto ApiResourceProperties { get; }
    }
}