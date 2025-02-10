using API.Identity.AuditLogging.Events;
using API.Identity.AuditLogging.Host.Dtos;

namespace API.Identity.AuditLogging.Host.Events
{
    public class ProductGetEvent : AuditEvent
    {
        public ProductDto Product { get; set; }
    }
}
