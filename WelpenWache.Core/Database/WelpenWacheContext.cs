﻿using Microsoft.EntityFrameworkCore;
using WelpenWache.Core.Database.Models;

namespace WelpenWache.Core.Database;

public class WelpenWacheContext(DbContextOptions<WelpenWacheContext> options) : DbContext(options) {
    public DbSet<Intern> Interns { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }
    public DbSet<AccessRequest> AccessRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<UserPermission>()
            .HasKey(p => new { p.Sid, p.Permission });

        modelBuilder.Entity<UserPermission>().Property(p => p.Permission).HasConversion<string>();
        
        modelBuilder.Entity<AccessRequest>()
            .Property(p => p.Status).HasConversion<string>();
    }
}