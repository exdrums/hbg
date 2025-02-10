using API.Identity.AuditLogging.Events;
using API.Identity.AuditLogging.Host.Dtos;

namespace API.Identity.AuditLogging.Host.Events
{
    public class GenericProductEvent<T1, T2, T3> : AuditEvent
    {
        public ProductDto Product { get; set; }
    }
}
