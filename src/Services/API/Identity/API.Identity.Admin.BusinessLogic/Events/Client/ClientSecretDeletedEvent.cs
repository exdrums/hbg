﻿using API.Identity.AuditLogging.Events;

namespace API.Identity.Admin.BusinessLogic.Events.Client
{
    public class ClientSecretDeletedEvent : AuditEvent
    {
        public int ClientId { get; set; }

        public int ClientSecretId { get; set; }

        public ClientSecretDeletedEvent(int clientId, int clientSecretId)
        {
            ClientId = clientId;
            ClientSecretId = clientSecretId;
        }
    }
}