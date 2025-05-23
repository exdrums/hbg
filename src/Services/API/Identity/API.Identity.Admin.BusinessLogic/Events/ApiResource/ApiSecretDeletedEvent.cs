﻿using API.Identity.AuditLogging.Events;

namespace API.Identity.Admin.BusinessLogic.Events.ApiResource
{
    public class ApiSecretDeletedEvent : AuditEvent
    {
        public int ApiResourceId { get; set; }

        public int ApiSecretId { get; set; }

        public ApiSecretDeletedEvent(int apiResourceId, int apiSecretId)
        {
            ApiResourceId = apiResourceId;
            ApiSecretId = apiSecretId;
        }
    }
}