﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

// Original file: https://github.com/IdentityServer/IdentityServer4/src/Services/IEventSink.cs
// Modified by Jan Škoruba

using System.Threading.Tasks;
using API.Identity.AuditLogging.Events;

namespace API.Identity.AuditLogging.Services
{
    public interface IAuditEventLoggerSink
    {
        Task PersistAsync(AuditEvent auditEvent);
    }
}