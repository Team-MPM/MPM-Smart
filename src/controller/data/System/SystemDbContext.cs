﻿using System.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.System;

public class SystemDbContext : IdentityDbContext<SystemUser>
{
    public SystemDbContext(DbContextOptions<SystemDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SystemDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
}