﻿using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using API.Identity.AuditLogging.EntityFramework.DbContexts;
using API.Identity.AuditLogging.EntityFramework.Entities;

namespace API.Identity.Admin.EntityFramework.Shared.DbContexts
{
    public class AdminAuditLogDbContext : DbContext, IAuditLoggingDbContext<AuditLog>
    {
        public AdminAuditLogDbContext(DbContextOptions<AdminAuditLogDbContext> dbContextOptions)
            : base(dbContextOptions)
        {

        }

        public Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }

        public DbSet<AuditLog> AuditLog { get; set; }
    }
}








