using API.Identity.AuditLogging.Events;
using API.Identity.Admin.BusinessLogic.Dtos.Configuration;

namespace API.Identity.Admin.BusinessLogic.Events.ApiScope
{
    public class ApiScopePropertyRequestedEvent : AuditEvent
    {
        public ApiScopePropertyRequestedEvent(int apiScopePropertyId, ApiScopePropertiesDto apiScopeProperty)
        {
            ApiScopePropertyId = apiScopePropertyId;
            ApiScopeProperty = apiScopeProperty;
        }

        public int ApiScopePropertyId { get; set; }

        public ApiScopePropertiesDto ApiScopeProperty { get; set; }
    }
}