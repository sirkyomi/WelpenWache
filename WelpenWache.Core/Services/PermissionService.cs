﻿using Microsoft.EntityFrameworkCore;
using WelpenWache.Core.Database;

namespace WelpenWache.Core.Services;

public class PermissionService {
    private readonly IDbContextFactory<WelpenWacheContext> _contextFactory;
    
    public PermissionService(IDbContextFactory<WelpenWacheContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<List<Permissions>> GetPermissionsForUserAsync(string windowsSid) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserPermissions
            .Where(p => p.Sid == windowsSid)
            .Select(p => p.Permission)
            .ToListAsync();
    }

    public async Task<bool> HasAnyPermissionAsync(string windowsSid) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserPermissions.AnyAsync(p => p.Sid == windowsSid);
    }

    public async Task AddPermissionAsync(string sid, Permissions permission) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var exists = await context.UserPermissions
            .AnyAsync(p => p.Sid == sid && p.Permission == permission);
        
        if (exists) {
            return;
        }

        context.UserPermissions.Add(new Database.Models.UserPermission {
            Sid = sid,
            Permission = permission
        });

        await context.SaveChangesAsync();
    }

    public async Task RemovePermissionAsync(string sid, Permissions permission) {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var userPermission = await context.UserPermissions
            .FirstOrDefaultAsync(p => p.Sid == sid && p.Permission == permission);
        
        if (userPermission != null) {
            context.UserPermissions.Remove(userPermission);
            await context.SaveChangesAsync();
        }
    }
}