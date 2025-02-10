using API.Identity.AuditLogging.Events;
using API.Identity.Admin.BusinessLogic.Dtos.Configuration;

namespace API.Identity.Admin.BusinessLogic.Events.ApiResource
{
    public class ApiResourcePropertyDeletedEvent : AuditEvent
    {
        public ApiResourcePropertyDeletedEvent(ApiResourcePropertiesDto apiResourceProperty)
        {
            ApiResourceProperty = apiResourceProperty;
        }

        public ApiResourcePropertiesDto ApiResourceProperty { get; set; }
    }
}