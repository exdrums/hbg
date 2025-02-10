using API.Identity.AuditLogging.Events;
using API.Identity.Admin.BusinessLogic.Dtos.Configuration;

namespace API.Identity.Admin.BusinessLogic.Events.Client
{
    public class ClientDeletedEvent : AuditEvent
    {
        public ClientDto Client { get; set; }

        public ClientDeletedEvent(ClientDto client)
        {
            Client = client;
        }
    }
}